using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Blockchain
{
    public class ParamsReader
    {
        private static readonly string NL = Environment.NewLine;

        public static Dictionary<string, dynamic> ReadParamsFromFile(string fileName)
        {
            // File wont be huge, therefore okay to read into memory
            var fileContents = File.ReadAllText(fileName);

            return ReadParamsFromString(fileContents);
        }

        public static Dictionary<string, dynamic> ReadParamsFromString(string paramsContent)
        {
            var myParams = new Dictionary<string, dynamic>();
            var x = 0;
            foreach (var line in paramsContent.Split(new[] {NL}, StringSplitOptions.None))
            {
                x++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var trimmedLine = line.Trim();

                // Look for comment, ignore everything after '#'
                var commentIndex = trimmedLine.IndexOf('#');
                // Line comment
                if (commentIndex == 0)
                    continue;

                var interestingPart = commentIndex > -1
                    ? trimmedLine.Substring(0, commentIndex).Trim()
                    : trimmedLine;
                var parts = interestingPart.Split('=');

                if (parts.Length != 2)
                {
                    var msg = parts.Length > 2 ? "more than one equals (=)" : "missing equals (=)";
                    Debug.WriteLine($"Invalid line ({x}), {msg}: \"{interestingPart}\"");
                    continue;
                }

                myParams[parts[0].Trim()] = ParseValue(parts[1].Trim());
            }
            return myParams;
        }

        public static string ParametersToString(Dictionary<string, dynamic> dict)
        {
            return string.Join(NL, dict.Select(d => $"{d.Key} = {ValueOut(d.Value)}")) + NL;
        }

        public static void ParametersToFile(string fileName, Dictionary<string, dynamic> dict)
        {
            var text = ParametersToString(dict);
            File.WriteAllText(fileName, text);
        }

        private static string ValueOut(dynamic value)
        {
            if (value == null)
                return "[null]";

            if (value is bool)
                return value.ToString().ToLower();

            return value.ToString();
        }

        private static dynamic ParseValue(string value)
        {
            bool b;
            if (bool.TryParse(value, out b))
                return b;

            int i;
            if (int.TryParse(value, out i))
                return i;

            double d;
            if (double.TryParse(value, out d))
                return d;

            if (value == "[null]")
                return null;

            // Hex values are stored as strings

            return value;
        }
    }
}