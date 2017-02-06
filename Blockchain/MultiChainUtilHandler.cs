using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Blockchain.Exceptions;
using Blockchain.Properties;

namespace Blockchain
{
    public static class MultiChainUtilHandler
    {
        public static async Task CreateBlockchain(string blockchainName)
        {
            var evotoDir = MultiChainHandler.GetAppDataFolder();
            var multichainUtilPath = Path.Combine(evotoDir, "multichain-util.exe");

            MultiChainHandler.EnsureFileExists(multichainUtilPath, Resources.multichain_util);

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
            var count = 1;
            while (!process.HasExited)
            {
                // 10s
                if (count++ > 100)
                {
                    process.Kill();
                    throw new CouldNotCreateBlockchainException("Process hung");
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            // TODO: Read output?
            if (errQueue.Count > 0)
                throw new CouldNotCreateBlockchainException(string.Join("\n", errQueue));
        }
    }
}