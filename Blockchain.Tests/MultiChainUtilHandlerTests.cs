using System.IO;
using System.Threading.Tasks;
using Blockchain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class MultiChainUtilHandlerTests
    {
        [TestMethod]
        public async Task Create_NotExist_Ok()
        {
            const string bName = "myBlockchain";
            var dir = Path.Combine(MultichainTools.GetAppDataFolder(), bName);

            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            await MultiChainUtilHandler.CreateBlockchain(bName);

            Assert.IsTrue(Directory.Exists(dir));
        }

        [TestMethod]
        public async Task Create_Exists_Exception()
        {
            const string bName = "myBlockchain";
            var dir = Path.Combine(MultichainTools.GetAppDataFolder(), bName);

            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            await MultiChainUtilHandler.CreateBlockchain(bName);

            // Attempt to create it again
            try
            {
                await MultiChainUtilHandler.CreateBlockchain(bName);
            }
            catch (CouldNotCreateBlockchainException e)
            {
                Assert.AreEqual("ERROR: Blockchain parameter set was not generated.", e.Message);
            }
        }
    }
}