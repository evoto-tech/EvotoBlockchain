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

        public Dictionary<string, MultichainModel> Connections { get; } = new Dictionary<string, MultichainModel>();

        public event EventHandler<EventArgs> OnConnect;

        /*****************************************************************************************/

        /// <summary>
        ///     Connect to a blockchain
        /// </summary>
        /// <param name="hostname">Blockchain host</param>
        /// <param name="blockchain">Blockchain name</param>
        /// <param name="port">Blockchain port</param>
        /// <param name="localPort">Blockchain local port</param>
        /// <param name="clean">Should clean local blockchain directory?</param>
        /// <returns></returns>
        public async Task<MultichainModel> Connect(string hostname, string blockchain, int port, int localPort, bool clean = true)
        {
            MultichainModel chain;
            if (!Connections.TryGetValue(blockchain, out chain))
            {
                chain = new MultichainModel(hostname, port, blockchain, RpcUser, MultiChainTools.RandomString(), localPort);
                Connections[blockchain] = chain;
            }

            var model = await RunDaemon(chain, clean);
            await ConnectRpc(model);

            return model;
        }

        /// <summary>
        ///     Runs multichaind in a background process, connecting to a specified blockchain.
        ///     Optionally callsback on successful launch, determined by multichaind's stdout/err.
        /// </summary>
        /// <param name="chain">Blockchain connection/status data</param>
        /// <param name="clean">Should clean local blockchain directory?</param>
        /// <returns>Blockchain connection/status data</returns>
        private static async Task<MultichainModel> RunDaemon(MultichainModel chain, bool clean)
        {
            if (chain.Process == null)
            {
                // First time the blockchain is being connected to, need to find a port to host RPC
                chain.RpcPort = MultiChainTools.GetNewPort(EPortType.Rpc);
            }
            else if (chain.Process.HasExited)
            {
                Debug.WriteLine($"Restarting Multichaind for chain: {chain.Name}!!");
            }
            else
            {
                return chain;
            }

            // Get working directory and multichaind.exe path
            var evotoDir = MultiChainTools.GetAppDataFolder(allowSubDir: false);
            var multichainDPath = Path.Combine(evotoDir, "multichaind.exe");
            MultiChainTools.EnsureFileExists(multichainDPath, Resources.multichaind);

            var dataDir = MultiChainTools.GetAppDataFolder();
            // Clean if required (multichain bug)
            if (clean)
                MultiChainTools.CleanBlockchain(dataDir, chain.Name);

            Debug.WriteLine($"Starting MultiChain connection to {chain.Name}@{chain.Hostname}:{chain.Port} ({chain.LocalPort})");
            Debug.WriteLine($"RPC Data: {RpcUser} : {chain.RpcPassword} : {chain.RpcPort}");
            var pArgs =
                $"{chain.Name}@{chain.Hostname}:{chain.Port} -daemon -datadir={dataDir} -server -port={chain.LocalPort}" +
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

            var taskCompletion = new TaskCompletionSource<bool>();

            // Connect to outputs
            chain.Process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data))
                    return;
                Debug.WriteLine($"Multichaind Error ({chain.Name}): {args.Data}");
            };
            chain.Process.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;
                Debug.WriteLine($"Multichaind ({chain.Name}): {e.Data}");
                if (e.Data.Contains("Node started"))
                    taskCompletion.SetResult(true);
            };

            // Launch process
            var success = chain.Process.Start();

            if (!success)
                throw new CouldNotStartProcessException();

            // Read outputs
            chain.Process.BeginOutputReadLine();
            chain.Process.BeginErrorReadLine();

            await Task.Run(() =>
            {
                if (!taskCompletion.Task.Wait(TimeSpan.FromMinutes(1)))
                {
                    chain.Process.Kill();
                    throw new CouldNotStartDaemonException("No Success Message");
                }
            });

            return chain;
        }

        /// <summary>
        ///     Disconnects from a blockchain (RPC) and stops the associated daemon
        /// </summary>
        /// <param name="chain">Blockchain details</param>
        public async Task DisconnectAndClose(MultichainModel chain)
        {
            await chain.DisconnectRpc();
            StopDaemon(chain);
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