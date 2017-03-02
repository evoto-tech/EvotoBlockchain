using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Exceptions;
using Blockchain.Properties;

namespace Blockchain
{
    public static class MultiChainUtilHandler
    {
        public static async Task CreateBlockchain(string blockchainName)
        {
            var evotoDir = MultiChainTools.GetAppDataFolder();
            var multichainUtilPath = Path.Combine(evotoDir, "multichain-util.exe");

            MultiChainTools.EnsureFileExists(multichainUtilPath, Resources.multichain_util);

            Debug.WriteLine($"Creating MultiChain: {blockchainName}");

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
                    Arguments = $"-datadir={evotoDir} create {blockchainName}"
                }
            };

            var outputQueue = new Queue<string>();
            var errQueue = new Queue<string>();
            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                Debug.WriteLine($"Multichain-util: {args.Data}");
                outputQueue.Enqueue(args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                Debug.WriteLine($"Multichain-util Error: {args.Data}");
                errQueue.Enqueue(args.Data);
            };

            // Go
            var success = process.Start();

            if (!success)
                throw new CouldNotCreateBlockchainException("Could not start process");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            // Wait for process to end
            await Task.Run(() =>
            {
                if (process.WaitForExit(10*1000))
                {
                    // Allow error events time to be processed
                    Thread.Sleep(100);
                    if (errQueue.Count > 0)
                        throw new CouldNotCreateBlockchainException(string.Join("\n", errQueue));
                }
                else
                {
                    // Process hanging
                    process.Kill();
                    throw new CouldNotCreateBlockchainException("Process timed out");
                }
            });
        }
    }
}