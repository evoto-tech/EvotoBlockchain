using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Blockchain.Properties;

namespace Blockchain
{
    public class MultiChainUtilHandler
    {
        public static void CreateBlockchain(string blockchainName)
        {
            var evotoDir = MultiChainHandler.GetAppDataFolder();
            var multichainUtilPath = Path.Combine(evotoDir, "multichain-util.exe");

            MultiChainHandler.EnsureFileExists(multichainUtilPath, Resources.multichain_util);

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
                        FileName = multichainUtilPath,
                        Arguments = $"-datadir={evotoDir}"
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