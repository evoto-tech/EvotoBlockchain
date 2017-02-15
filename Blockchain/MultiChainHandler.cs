using System;
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
        private readonly string ChainHost;
        private const int ChainPort = 7211;
        private const string RpcUser = "evoto";
        private const int RpcPort = 24533;

        private static readonly Random Random = new Random();
        private readonly string _password = RandomString(10);
        private MultiChainClient _client;
        private bool _connected;
        private Process _process;
        public event EventHandler<EventArgs> OnConnect;

        public MultiChainHandler(string hostname)
        {
            ChainHost = hostname;
        }

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

        public async Task Connect(string blockchain)
        {
            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    await RunDaemon(blockchain, ConnectRpc);
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

        private async Task ConnectRpc(string chainName)
        {
            _client = new MultiChainClient("127.0.0.1", RpcPort, false, RpcUser, _password, chainName);

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

        private static async Task WatchProcess(string chainName, DataReceivedEventArgs e, Func<string, Task> successCallback)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            Debug.WriteLine($"Multichaind: {e.Data}");
            if (e.Data.Contains("Node started"))
                await successCallback(chainName);
        }

        private async Task RunDaemon(string chainName, Func<string, Task> successCallback)
        {
            if (_process != null)
                if (_process.HasExited)
                    Debug.WriteLine("Restarting Multichain!!");
                else
                    await successCallback(chainName);

            var evotoDir = GetAppDataFolder();
            var multichainDPath = Path.Combine(evotoDir, "multichaind.exe");

            EnsureFileExists(multichainDPath, Resources.multichaind);

            // TODO: Bug with multichain, have to delete existing chain directory
            var chainDir = Path.Combine(evotoDir, chainName);

            if (Directory.Exists(chainDir))
                Directory.Delete(chainDir, true);

            Debug.WriteLine("Starting MultiChain");
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
                        $"{chainName}@{ChainHost}:{ChainPort} -daemon -datadir={evotoDir} -server -rpcuser={RpcUser} -rpcpassword={_password} -rpcport={RpcPort}"
                }
            };

            _process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                Debug.WriteLine($"Multichaind Error: {args.Data}");
            };
            _process.OutputDataReceived += async (sender, e) => await WatchProcess(chainName, e, successCallback);

            // Go
            var success = _process.Start();

            if (!success)
                throw new SystemException();

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public static void EnsureFileExists(string filePath, byte[] file)
        {
            try
            {
                if (!File.Exists(filePath)) File.WriteAllBytes(filePath, file);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't get file: {filePath}");
                Debug.WriteLine(e);
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