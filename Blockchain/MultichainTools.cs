using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Blockchain.Models;

namespace Blockchain
{
    public static class MultiChainTools
    {
        public const string ROOT_STREAM_NAME = "root";
        public const string QUESTIONS_KEY = "questions";

        // This can be set to allow Api and Client to be ran on the same machine
        public static string SubDirectory = "";

        private static readonly Random Random = new Random();

        /// <summary>
        ///     Checks if a file exists in a specified location,
        ///     if no file exists, creates it based on its byte array content
        /// </summary>
        /// <param name="filePath">The path to look for the file</param>
        /// <param name="file">Bytes of file, to create if not exists</param>
        public static void EnsureFileExists(string filePath, byte[] file)
        {
            try
            {
                if (!File.Exists(filePath))
                    File.WriteAllBytes(filePath, file);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Couldn't get file: {filePath}");
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        ///     Deletes a blockchain's directory (usually in AppData).
        ///     Necessary for reconnecting to a blockchain not created locally.
        ///     Only necessary due to some form of bug in Multichain
        /// </summary>
        /// <param name="multichainDir">Multichain Working Directory</param>
        /// <param name="chainName"></param>
        public static void CleanBlockchain(string multichainDir, string chainName)
        {
            var chainDir = Path.Combine(multichainDir, chainName);

            if (Directory.Exists(chainDir))
                Directory.Delete(chainDir, true);
        }

        /// <summary>
        ///     Creates a random string using characters A-Z and 0-9 of specified length
        /// </summary>
        /// <param name="length">Length of password to create</param>
        /// <returns>Randomly generated password of specified length</returns>
        public static string RandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        ///     Returns the appdata folder, with optional relative suffix.
        ///     Throws a SystemException if APPDATA environment variable is not set.
        /// </summary>
        /// <param name="relative">Relative path to add on to end of AppData</param>
        /// <returns>Path of AppData, with optional suffix appended</returns>
        public static string GetAppDataFolder(string relative = "Evoto")
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
                throw new SystemException("APPDATA Must be set");

            if (relative != null)
                return Path.Combine(appData, relative, SubDirectory);
            return Path.Combine(appData, SubDirectory);
        }

        /// <summary>
        ///     Returns the next available port for specified use.
        ///     Starts from a predefined value for enum
        /// </summary>
        /// <returns>An available TPC Port</returns>
        public static int GetNewPort(EPortType type)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            while (true)
            {
                // Get port from tool
                var port = PortTypeUtils.GetPortNumber(type);
                // Check if our desired port is in use
                if (tcpConnInfoArray.All(t => t.LocalEndPoint.Port != port))
                    // This port is good
                    return port;
            }
        }
    }
}