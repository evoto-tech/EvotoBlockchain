using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class ParamsReaderTests
    {
        private static readonly string NL = Environment.NewLine;

        [TestMethod]
        public void ReadParamsFromFile_FileExists_Success()
        {
            const string fileName = "test.txt";
            try
            {
                var testText = $"something-interesting = 1.2{NL}" +
                               $" another-thing = false # comment about the thing {NL}" +
                               $"my-info = 2{NL}" +
                               $"    asdf = AD43     {NL}" +
                               $"my-string = something long and interesting{NL}" +
                               $"# commented-value = 2{NL}" +
                               $"something-null = [null]";

                File.WriteAllText(fileName, testText);

                var dict = ParamsReader.ReadParamsFromFile(fileName);

                Assert.AreEqual(1.2, dict["something-interesting"]);
                Assert.AreEqual(false, dict["another-thing"]);
                Assert.AreEqual(2, dict["my-info"]);
                Assert.AreEqual("AD43", dict["asdf"]);
                Assert.AreEqual("something long and interesting", dict["my-string"]);
                Assert.IsNull(dict["something-null"]);
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
        public void ReadParamsFromString_Valid_AccurateDict()
        {
            var testText = $"something-interesting = 1.2{NL}" +
                           $" another-thing = false # comment about the thing {NL}" +
                           $"my-info = 2{NL}" +
                           $"    asdf = AD43     {NL}" +
                           $"my-string = something long and interesting{NL}" +
                           $"# commented-value = 2{NL}" +
                           $"something-null = [null]";

            var dict = ParamsReader.ReadParamsFromString(testText);

            Assert.AreEqual(1.2, dict["something-interesting"]);
            Assert.AreEqual(false, dict["another-thing"]);
            Assert.AreEqual(2, dict["my-info"]);
            Assert.AreEqual("AD43", dict["asdf"]);
            Assert.AreEqual("something long and interesting", dict["my-string"]);
            Assert.IsNull(dict["something-null"]);
            Assert.IsFalse(dict.ContainsKey("commented-value"));
        }

        [TestMethod]
        public void ParametersToString_Valid_AccurateString()
        {
            var myDict = new Dictionary<string, dynamic>
            {
                {"my-key", false},
                {"your-key", "something cool"},
                {"another-key", 1.2},
                {"something-cool", 0},
                {"null-info", null}
            };

            var myString = ParamsReader.ParametersToString(myDict);

            Assert.AreEqual($"my-key = false{NL}" +
                            $"your-key = something cool{NL}" +
                            $"another-key = 1.2{NL}" +
                            $"something-cool = 0{NL}" +
                            $"null-info = [null]{NL}", myString);
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
                    {"something-cool", 0},
                    {"null-info", null}
                };

                ParamsReader.ParametersToFile(fileName, myDict);

                var myString = File.ReadAllText(fileName);

                Assert.AreEqual($"my-key = false{NL}" +
                                $"your-key = something cool{NL}" +
                                $"another-key = 1.2{NL}" +
                                $"something-cool = 0{NL}" +
                                $"null-info = [null]{NL}", myString);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}