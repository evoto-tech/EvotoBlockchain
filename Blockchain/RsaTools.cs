using System;
using System.IO;
using System.Text;
using Blockchain.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Blockchain
{
    public static class RsaTools
    {
        private const string KEY_FOLDER = ".evoto";

        public static BlindedMessage BlindMessage(string message, AsymmetricKeyParameter publicKey)
        {
            var rsaPub = publicKey as RsaKeyParameters;
            if (rsaPub == null)
                throw new ArgumentException(nameof(publicKey));

            var r = GenerateRandomRelativelyPrimeBigInt(rsaPub.Modulus);
            var rPowE = r.ModPow(rsaPub.Exponent, rsaPub.Modulus);

            var m = MsgToRaw(message);
            return new BlindedMessage
            {
                Random = r,
                // m' = mr^e (Mod N)
                Blinded = m.Multiply(rPowE).Mod(rsaPub.Modulus)
            };
        }

        public static BigInteger SignBlindedMessage(BigInteger blindedMessage, AsymmetricKeyParameter privateKey)
        {
            var rsaPriv = privateKey as RsaKeyParameters;
            if (rsaPriv == null)
                throw new ArgumentException(nameof(privateKey));

            // s' = (m')^d (Mod N)
            return blindedMessage.ModPow(rsaPriv.Exponent, rsaPriv.Modulus);
        }

        public static bool VerifySignature(string message, BigInteger signature, AsymmetricKeyParameter privateKey)
        {
            var rsaPriv = privateKey as RsaKeyParameters;
            if (rsaPriv == null)
                throw new ArgumentException(nameof(privateKey));

            var rawMsg = MsgToRaw(message);
            var sig = SignBlindedMessage(rawMsg, privateKey);

            // Signature of the unblinded message == signature of the regular message
            return signature.Equals(sig);
        }

        public static BigInteger UnblindMessage(BigInteger blindSignature, BigInteger random,
            AsymmetricKeyParameter publicKey)
        {
            var rsaPub = publicKey as RsaKeyParameters;
            if (rsaPub == null)
                throw new ArgumentException(nameof(publicKey));

            // r^-1 (Mod N)
            var inv = random.ModInverse(rsaPub.Modulus);

            // s = (s')(r^-1) (Mod N)
            return blindSignature.Multiply(inv).Mod(rsaPub.Modulus);
        }

        private static BigInteger MsgToRaw(string msg)
        {
            return new BigInteger(Encoding.UTF8.GetBytes(msg));
        }

        private static BigInteger GenerateRandomRelativelyPrimeBigInt(BigInteger mod)
        {
            BigInteger tempRandomBigInt;
            do
            {
                tempRandomBigInt = GenerateRandomBigInt();
            } while (AreRelativelyPrime(tempRandomBigInt, mod));

            return tempRandomBigInt;
        }

        private static BigInteger GenerateRandomBigInt(int size = 20)
        {
            var randomBytes = new byte[size];
            var random = new SecureRandom();

            random.NextBytes(randomBytes);
            return new BigInteger(1, randomBytes);
        }

        private static bool AreRelativelyPrime(BigInteger first, BigInteger second)
        {
            var one = BigInteger.One;
            var gcd = first.Gcd(second);
            return !gcd.Equals(one) || (first.CompareTo(second) >= 0) || (first.CompareTo(one) <= 0);
        }

        public static AsymmetricCipherKeyPair LoadKeysFromFile(string name)
        {
            // Relies on existing "installation" of private key in home dir
            var drive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            if (drive == null)
                throw new Exception("HOMEDRIVE not set");

            var folder = Environment.GetEnvironmentVariable("HOMEPATH");
            if (folder == null)
                throw new Exception("HOMEPATH not set");

            if (!name.Contains("."))
                name += ".pem";

            var privateKeyFile = drive + Path.Combine(folder, KEY_FOLDER, name);
            if (!File.Exists(privateKeyFile))
                throw new Exception($"Key not found ({name})");

            using (var reader = File.OpenText(privateKeyFile))
            {
                return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
            }
        }
    }
}