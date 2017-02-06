using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Blockchain
{
    public class MultiChainUtilHandler
    {
        public static void CreateBlockchain(string blockchainName)
        {
            var datadir = MultiChainHandler.GetAppDataFolder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "multichain-util.exe");

            Debug.WriteLine($"Creating MultiChain: {blockchainName}");
            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        // Stop the process from opening a new window
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,

                        // Setup executable and parameters
                        FileName = path,
                        Arguments = $"-datadir={datadir}"
                    }
                };

                process.OutputDataReceived += (sender, args) => { Debug.WriteLine($"Multichain-util: {args.Data}"); };
                process.ErrorDataReceived +=
                    (sender, args) => { Debug.WriteLine($"Multichain-util Error: {args.Data}"); };

                // Go
                var success = process.Start();

                if (!success)
                    throw new SystemException();
            }
            catch (Exception e)
            {
                throw new WarningException(e.Message);
            }
        }
    }
}