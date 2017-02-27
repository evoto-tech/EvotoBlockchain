using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class ParamsReaderTests
    {
        [TestMethod]
        public void ReadParamsFromFile_FileExists_Success()
        {
            const string fileName = "test.txt";
            try
            {
                var NL = Environment.NewLine;
                var testText = $"something-interesting = 1.2{NL}" +
                               $" another-thing = false # comment about the thing {NL}" +
                               $"my-info = 2{NL}" +
                               $"    asdf = AD43     {NL}" +
                               $"my-string = something long and interesting{NL}" +
                               "# commented-value = 2";

                File.WriteAllText(fileName, testText);

                var dict = ParamsReader.ReadParamsFromFile(fileName);

                Assert.AreEqual(1.2, dict["something-interesting"]);
                Assert.AreEqual(false, dict["another-thing"]);
                Assert.AreEqual(2, dict["my-info"]);
                Assert.AreEqual("AD43", dict["asdf"]);
                Assert.AreEqual("something long and interesting", dict["my-string"]);
                Assert.IsFalse(dict.ContainsKey("commented-value"));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ReadParamsFromFile_FileNotExist_Exception()
        {
            ParamsReader.ReadParamsFromFile("file2.txt");
        }

        [TestMethod]
        public void ParametersToString_Valid_AccurateString()
        {
            var myDict = new Dictionary<string, dynamic>
            {
                {"my-key", false},
                {"your-key", "something cool"},
                {"another-key", 1.2},
                {"something-cool", 0}
            };

            var myString = ParamsReader.ParametersToString(myDict);

            var NL = Environment.NewLine;
            Assert.AreEqual($"my-key = false{NL}" +
                            $"your-key = something cool{NL}" +
                            $"another-key = 1.2{NL}" +
                            $"something-cool = 0", myString);
        }

        [TestMethod]
        public void ParametersToFile_Valid_FileOutputValid()
        {
            const string fileName = "file3.txt";
            try
            {
                var myDict = new Dictionary<string, dynamic>
                {
                    {"my-key", false},
                    {"your-key", "something cool"},
                    {"another-key", 1.2},
                    {"something-cool", 0}
                };

                ParamsReader.ParametersToFile(fileName, myDict);

                var myString = File.ReadAllText(fileName);

                var NL = Environment.NewLine;
                Assert.AreEqual($"my-key = false{NL}" +
                                $"your-key = something cool{NL}" +
                                $"another-key = 1.2{NL}" +
                                $"something-cool = 0", myString);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}