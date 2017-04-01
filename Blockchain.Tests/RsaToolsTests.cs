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
    }
}