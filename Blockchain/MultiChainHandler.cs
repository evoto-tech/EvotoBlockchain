using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blockchain.Properties;
using MultiChainLib;

namespace Blockchain
{
    public class MultiChainHandler : IMultiChainHandler
    {
        private const string ChainHost = "michaelfiford.com";
        private const int ChainPort = 7211;
        private const string ChainName = "chain2";
        private const string RpcUser = "evoto";
        private const int RpcPort = 24533;

        private static readonly Random Random = new Random();
        private readonly string _password = RandomString(10);
        private MultiChainClient _client;
        private bool _connected;
        private Process _process;
        public event EventHandler<EventArgs> OnConnect;

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                if (value)
                    OnConnect?.Invoke(this, null);
                _connected = value;
            }
        }

        public async Task Connect()
        {
            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    await RunDaemon(ConnectRpc);
                    Close();
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void DisconnectAndClose()
        {
            Task.Factory.StartNew(async () =>
            {
                await Disconnect();
                Close();
            });
        }

        public void Close()
        {
            StopDaemon();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private async Task ConnectRpc()
        {
            _client = new MultiChainClient("127.0.0.1", RpcPort, false, RpcUser, _password, ChainName);

            Debug.WriteLine("Attempting to connect to MultiChain using RPC");

            try
            {
                var info = await _client.GetInfoAsync();

                Debug.WriteLine($"Connected to {info.Result.ChainName}!");

                Connected = true;
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("Could not connect to MultiChain via RPC");
                Debug.WriteLine(e.Message);
                Connected = false;
            }
        }

        private static async Task WatchProcess(object sender, DataReceivedEventArgs e, Func<Task> successCallback)
        {
            Debug.WriteLine($"Multichaind: {e.Data}");
            if (e.Data.Contains("Node started"))
                await successCallback();
        }

        private async Task RunDaemon(Func<Task> successCallback)
        {
            if (_process != null)
                if (_process.HasExited)
                    Debug.WriteLine("Restarting Multichain!!");
                else
                    await successCallback();

            var evotoDir = GetAppDataFolder();
            var multichainDPath = Path.Combine(evotoDir, "multichaind.exe");

            try
            {
                var file = Resources.multichaind;
                if (!File.Exists(multichainDPath)) File.WriteAllBytes(multichainDPath, file);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }


            // TODO: Bug with multichain, have to delete existing chain directory
            var chainDir = Path.Combine(evotoDir, ChainName);

            if (Directory.Exists(chainDir))
                Directory.Delete(chainDir, true);

            Debug.WriteLine("Starting MultiChain");
            try
            {
                _process = new Process
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
                        Arguments =
                            $"{ChainName}@{ChainHost}:{ChainPort} -daemon -datadir={evotoDir} -server -rpcuser={RpcUser} -rpcpassword={_password} -rpcport={RpcPort}"
                    }
                };

                Debug.WriteLine(_process.StartInfo.Arguments);

                _process.ErrorDataReceived +=
                    (sender, args) => { Debug.WriteLine($"Multichaind Error: {args.Data}"); };
                _process.OutputDataReceived += async (sender, e) => await WatchProcess(sender, e, successCallback);

                // Go
                var success = _process.Start();

                if (!success)
                    throw new SystemException();

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                await successCallback();
            }
            catch (Exception e)
            {
                throw new WarningException(e.Message);
            }
        }

        private async Task Disconnect()
        {
            Debug.WriteLine($"Disconnecting (Connected: {_connected})");

            if (!_connected)
                return;

            await _client.StopAsync();
        }

        private void StopDaemon()
        {
            Debug.WriteLine(
                $"Stopping MultiChain Daemon (Process Exists: {_process != null}, Exited: {_process?.HasExited})");

            if ((_process == null) || _process.HasExited)
                return;

            _process.Close();
        }

        public static string GetAppDataFolder(string relative = "Evoto")
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData == null)
                throw new SystemException("APPDATA Must be set");

            if (relative != null)
                return Path.Combine(appData, relative);
            return appData;
        }

        #region Methods

        public async Task<BlockchainInfoResponse> GetInfo()
        {
            var res = await _client.GetBlockchainInfoAsync();
            return res.Result;
        }

        public async Task<BlockResponse> GetGenesisBlock()
        {
            // Should be possible to get the block by height but client doesn't seem to handle it, so get hash first
            var hashRes = await _client.GetBlockHashAsync(0);
            var res = await _client.GetBlockVerboseAsync(hashRes.Result);

            return res.Result;
        }

        public async Task<string> GetBlock(string hash)
        {
            var res = await _client.GetBlockAsync(hash);
            return res.Result;
        }

        public async Task<BlockResponse> GetBlockFull(string hash)
        {
            var res = await _client.GetBlockVerboseAsync(hash);
            return res.Result;
        }

        public async Task<VerboseTransactionResponse> GetTransaction(string txId)
        {
            var res = await _client.GetRawTransactionVerboseAsync(txId);
            return res.Result;
        }

        public async Task<string> WriteTransaction(object something)
        {
            var tx = await _client.CreateRawTransactionAync();
            var txId = tx.Result;
            await _client.AppendRawDataAsync(txId, something);
            await _client.SendRawTransactionAsync(txId);
            return txId;
        }

        #endregion
    }
}