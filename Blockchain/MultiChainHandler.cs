using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Blockchain.Exceptions;
using Blockchain.Models;
using Blockchain.Properties;

namespace Blockchain
{
    public class MultiChainHandler
    {
        private const string RpcUser = "evoto";

        private readonly Dictionary<string, MultichainModel> _connections = new Dictionary<string, MultichainModel>();

        public event EventHandler<EventArgs> OnConnect;

        /*****************************************************************************************/

        /// <summary>
        ///     Connect to a blockchain
        /// </summary>
        /// <param name="hostname">Blockchain host</param>
        /// <param name="blockchain">Blockchain name</param>
        /// <param name="port">Blockchain port</param>
        /// <param name="clean">Should clean local blockchain directory?</param>
        /// <returns></returns>
        public async Task<MultichainModel> Connect(string hostname, string blockchain, int port, bool clean = true)
        {
            MultichainModel chain;
            if (!_connections.TryGetValue(blockchain, out chain))
            {
                chain = new MultichainModel(hostname, port, blockchain, RpcUser, MultichainTools.RandomString());
                _connections[blockchain] = chain;
            }

            return await RunDaemon(chain, clean, ConnectRpc);
        }

        /// <summary>
        ///     Runs multichaind in a background process, connecting to a specified blockchain.
        ///     Optionally callsback on successful launch, determined by multichaind's stdout/err.
        /// </summary>
        /// <param name="chain">Blockchain connection/status data</param>
        /// <param name="clean">Should clean local blockchain directory?</param>
        /// <param name="successCallback">Callsback on successful launch</param>
        /// <returns>Blockchain connection/status data</returns>
        private static async Task<MultichainModel> RunDaemon(MultichainModel chain, bool clean,
            Func<MultichainModel, Task> successCallback = null)
        {
            if (chain.Process == null)
            {
                // First time the blockchain is being connected to, need to find a port to host RPC
                chain.RpcPort = MultichainTools.GetNewRpcPort();
            }
            else if (chain.Process.HasExited)
            {
                Debug.WriteLine($"Restarting Multichaind for chain: {chain.Name}!!");
            }
            else
            {
                if (successCallback != null)
                    await successCallback(chain);
                return chain;
            }

            // Get working directory and multichaind.exe path
            var evotoDir = MultichainTools.GetAppDataFolder();
            var multichainDPath = Path.Combine(evotoDir, "multichaind.exe");
            MultichainTools.EnsureFileExists(multichainDPath, Resources.multichaind);

            // Clean if required (multichain bug)
            if (clean)
                MultichainTools.CleanBlockchain(evotoDir, chain.Name);

            Debug.WriteLine($"Starting MultiChain connection to {chain.Name}@{chain.Hostname}:{chain.Port}");
            Debug.WriteLine($"RPC Data: {RpcUser} : {chain.RpcPassword} : {chain.RpcPort}");
            var pArgs =
                $"{chain.Name}@{chain.Hostname}:{chain.Port} -daemon -datadir={evotoDir} -server" +
                $" -rpcuser={RpcUser} -rpcpassword={chain.RpcPassword} -rpcport={chain.RpcPort}";
            chain.Process = new Process
            {
                StartInfo =
                {
                    // Stop the process from opening a new window
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,

                    // Setup executable and parameters
                    FileName = multichainDPath,
                    Arguments = pArgs
                }
            };

            // Connect to outputs
            chain.Process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data))
                    return;
                Debug.WriteLine($"Multichaind Error: {args.Data}");
            };
            chain.Process.OutputDataReceived += async (sender, e) => await WatchProcess(chain, e, successCallback);

            // Launch process
            var success = chain.Process.Start();

            if (!success)
                throw new CouldNotStartProcessException();

            // Read outputs
            chain.Process.BeginOutputReadLine();
            chain.Process.BeginErrorReadLine();

            return chain;
        }

        /// <summary>
        ///     Disconnects from a blockchain (RPC) and stops the associated daemon
        /// </summary>
        /// <param name="chain">Blockchain details</param>
        public void DisconnectAndClose(MultichainModel chain)
        {
            Task.Factory.StartNew(async () =>
            {
                await chain.DisconnectRpc();
                StopDaemon(chain);
            });
        }

        /// <summary>
        ///     Connects to a blockchain using RPC
        /// </summary>
        /// <param name="chain">Blockchain details</param>
        private async Task ConnectRpc(MultichainModel chain)
        {
            try
            {
                Debug.WriteLine($"Attempting to connect to MultiChain {chain.Name} using RPC");
                await chain.ConnectRpc();

                Debug.WriteLine($"Connected to {chain.Name}!");
                
                // Dispatch to event delegate(s)
                OnConnect?.Invoke(chain, EventArgs.Empty);
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($"Could not connect to MultiChain {chain.Name} via RPC");
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        ///     Watches the output from a process, waiting for the success message
        /// </summary>
        /// <param name="chain">Blockchain details</param>
        /// <param name="e">Event args</param>
        /// <param name="successCallback">Callback on success</param>
        private static async Task WatchProcess(MultichainModel chain, DataReceivedEventArgs e,
            Func<MultichainModel, Task> successCallback)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            Debug.WriteLine($"Multichaind ({chain.Name}): {e.Data}");
            if (e.Data.Contains("Node started"))
                await successCallback(chain);
        }


        private static void StopDaemon(MultichainModel chain)
        {
            Debug.WriteLine(
                $"Stopping MultiChain Daemon (Process Exists: {chain.Process != null}, Exited: {chain.Process?.HasExited})");

            if ((chain.Process == null) || chain.Process.HasExited)
                return;

            chain.Process.Close();
        }
    }
}