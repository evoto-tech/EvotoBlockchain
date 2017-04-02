using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class RsaToolsTests
    {
        [TestMethod]
        public void LoadKeysFromFile_Success()
        {
            var keysStored = RsaTools.LoadKeysFromFile("test");

            Assert.IsNotNull(keysStored.Private);
            Assert.IsTrue(keysStored.Private.IsPrivate);

            Assert.IsNotNull(keysStored.Public);
            Assert.IsFalse(keysStored.Public.IsPrivate);
        }

        [TestMethod]
        public void BlindSignature_KeysFromFile_Success()
        {
            var serverKey = RsaTools.LoadKeysFromFile("test");

            // Client
            const string message = "abcdefghijklmnopqrstuvwxyz0123456789";
            var blindedMessage = RsaTools.BlindMessage(message, serverKey.Public);

            // Server
            var blindSig = RsaTools.SignBlindedMessage(blindedMessage.Blinded, serverKey.Private);

            // Client
            var signedToken = RsaTools.UnblindMessage(blindSig, blindedMessage.Random, serverKey.Public);

            // Server
            Assert.IsTrue(RsaTools.VerifySignature(message, signedToken, serverKey.Private));
        }

        [TestMethod]
        public void CreateKey_Success()
        {
            var key = RsaTools.CreateKeyAndSave("test");
            Assert.IsNotNull(key);
        }

        [TestMethod]
        public void CreatePublicKeyToStringAndBack()
        {
            var key = RsaTools.CreateKey();
            var str = RsaTools.KeyToString(key.Public);
            var key2 = RsaTools.PublicKeyFromString(str);
            
            Assert.AreEqual(key.Public, key2);
        }

        [TestMethod]
        public void CreateKeypairToStringAndBack()
        {
            var key = RsaTools.CreateKey();
            var str = RsaTools.KeyToString(key.Private);
            var key2 = RsaTools.KeyPairFromString(str);

            Assert.AreEqual(key.Public, key2.Public);
            Assert.AreEqual(key.Private, key2.Private);
        }
    }
}