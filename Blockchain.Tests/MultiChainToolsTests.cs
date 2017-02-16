using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class MultiChainToolsTests
    {
        [TestMethod]
        public void RandomString_20_Length20()
        {
            var rs = MultiChainTools.RandomString(20);
            Assert.AreEqual(20, rs.Length);
        }

        [TestMethod]
        public void RandomString_1_Length1()
        {
            var rs = MultiChainTools.RandomString(1);
            Assert.AreEqual(1, rs.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(SystemException))]
        public void GetAppDataFolder_NotSet_SystemException()
        {
            var old = Environment.GetEnvironmentVariable("APPDATA");
            try
            {
                Environment.SetEnvironmentVariable("APPDATA", null);
                MultiChainTools.GetAppDataFolder();
            }
            catch
            {
                Environment.SetEnvironmentVariable("APPDATA", old);
                throw;
            }
        }

        [TestMethod]
        public void GetAppDataFolder_Set_ReturnsAppDataFolder()
        {
            var old = Environment.GetEnvironmentVariable("APPDATA");

            const string fakeFolder = @"C:\appdata";
            Environment.SetEnvironmentVariable("APPDATA", fakeFolder);

            var folder = MultiChainTools.GetAppDataFolder(null);

            Environment.SetEnvironmentVariable("APPDATA", old);
            Assert.AreEqual(fakeFolder, folder);
        }

        [TestMethod]
        public void GetAppDataFolder_EmptyStringRelative_ReturnsAppDataFolder()
        {
            var old = Environment.GetEnvironmentVariable("APPDATA");

            const string fakeFolder = @"C:\appdata";
            Environment.SetEnvironmentVariable("APPDATA", fakeFolder);

            var folder = MultiChainTools.GetAppDataFolder("");

            Environment.SetEnvironmentVariable("APPDATA", old);
            Assert.AreEqual(fakeFolder, folder);
        }

        [TestMethod]
        public void GetAppDataFolder_Relative_ReturnsRelativeAppDataFolder()
        {
            var old = Environment.GetEnvironmentVariable("APPDATA");

            const string fakeFolder = @"C:\appdata";
            const string fakeSubFolder = "testing";
            Environment.SetEnvironmentVariable("APPDATA", fakeFolder);

            var folder = MultiChainTools.GetAppDataFolder(fakeSubFolder);

            Environment.SetEnvironmentVariable("APPDATA", old);
            Assert.AreEqual(Path.Combine(fakeFolder, fakeSubFolder), folder);
        }
    }
}