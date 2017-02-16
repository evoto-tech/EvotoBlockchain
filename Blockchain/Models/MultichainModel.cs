using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MultiChainLib;

namespace Blockchain.Models
{
    public class MultichainModel
    {
        public string Hostname { get; }
        public int Port { get; }
        public string Name { get; }

        public string RpcUser { get; }
        public int RpcPort { get; set; }
        public string RpcPassword { get; }

        public Process Process { get; set; }
        public bool Connected { get; private set; }

        public MultiChainClient RpcClient { get; set; }

        public MultichainModel(string hostname, int port, string name, string rpcUser, string rpcPassword)
        {
            Hostname = hostname;
            Port = port;
            Name = name;
            RpcUser = rpcUser;
            RpcPassword = rpcPassword;
        }

        #region Setup/Teardown

        /// <summary>
        /// Disconnects RPC Client from multichaind
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectRpc()
        {
            Debug.WriteLine($"Disconnecting (Connected: {Connected})");

            if (!Connected || (RpcClient == null))
                return;

            await RpcClient.StopAsync();
            RpcClient = null;
            Connected = false;
        }

        /// <summary>
        /// Connects to a blockchain using RPC
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot connect to multichain</exception>
        public async Task ConnectRpc()
        {
            if (RpcClient != null && Connected)
                return;

            // Reset, in case we encounter an exception
            Connected = false;
            RpcClient = new MultiChainClient("127.0.0.1", RpcPort, false, RpcUser, RpcPassword, Name);

            await RpcClient.GetInfoAsync();
            Connected = true;

        }

        #endregion

        #region Methods

        public async Task<BlockchainInfoResponse> GetInfo()
        {
            var res = await RpcClient.GetBlockchainInfoAsync();
            return res.Result;
        }

        public async Task<BlockResponse> GetGenesisBlock()
        {
            // Should be possible to get the block by height but client doesn't seem to handle it, so get hash first
            var hashRes = await RpcClient.GetBlockHashAsync(0);
            var res = await RpcClient.GetBlockVerboseAsync(hashRes.Result);

            return res.Result;
        }

        public async Task<string> GetBlock(string hash)
        {
            var res = await RpcClient.GetBlockAsync(hash);
            return res.Result;
        }

        public async Task<BlockResponse> GetBlockFull(string hash)
        {
            var res = await RpcClient.GetBlockVerboseAsync(hash);
            return res.Result;
        }

        public async Task<VerboseTransactionResponse> GetTransaction(string txId)
        {
            var res = await RpcClient.GetRawTransactionVerboseAsync(txId);
            return res.Result;
        }

        public async Task<string> WriteTransaction(object something)
        {
            var tx = await RpcClient.CreateRawTransactionAync();
            var txId = tx.Result;
            await RpcClient.AppendRawDataAsync(txId, something);
            await RpcClient.SendRawTransactionAsync(txId);
            return txId;
        }

        #endregion
    }
}