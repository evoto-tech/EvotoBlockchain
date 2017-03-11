﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiChainLib.Client;
using MultiChainLib.Model;
using Newtonsoft.Json;

namespace Blockchain.Models
{
    public class MultichainModel
    {
        public MultichainModel(string hostname, int port, string name, string rpcUser, string rpcPassword, int localPort,
            int rpcPort)
        {
            Hostname = hostname;
            Port = port;
            Name = name;
            RpcUser = rpcUser;
            RpcPassword = rpcPassword;
            LocalPort = localPort;
            RpcPort = rpcPort;
        }

        public string Hostname { get; }
        public int Port { get; }

        public string Name { get; }
        public int LocalPort { get; }

        public string RpcUser { get; }
        public int RpcPort { get; set; }
        public string RpcPassword { get; }

        public Process Process { get; set; }
        public bool Connected { get; private set; }

        public MultiChainClient RpcClient { get; set; }

        #region Setup/Teardown

        /// <summary>
        ///     Disconnects RPC Client from multichaind
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectRpc()
        {
            Debug.WriteLine($"Disconnecting from {Name} (Connected: {Connected})");

            if (!Connected || (RpcClient == null))
                return;

            await RpcClient.StopAsync();
            RpcClient = null;
            Connected = false;
        }

        /// <summary>
        ///     Connects to a blockchain using RPC
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot connect to multichain</exception>
        public async Task ConnectRpc()
        {
            if ((RpcClient != null) && Connected)
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

        public async Task<string> WriteTransaction(
            IEnumerable<CreateRawTransactionTxIn> txIds,
            IEnumerable<CreateRawTransactionAmount> assets,
            object data = null)
        {
            var blobRes = await RpcClient.CreateRawTransactionAync(txIds, assets);
            var blob = blobRes.Result;
            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);
                var bytes = Encoding.UTF8.GetBytes(jsonData);
                blobRes = await RpcClient.AppendRawDataAsync(blob, MultiChainClient.FormatHex(bytes));
                blob = blobRes.Result;
            }
            var signedRes = await RpcClient.SignRawTransactionAsync(blob);
            var txId = await RpcClient.SendRawTransactionAsync(signedRes.Result.Hex);
            return txId.Result;
        }

        public async Task<string> WriteToStream(string stream, string key, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(jsonData);
            var res = await RpcClient.PublishAsync(stream, key, bytes);
            return res.Result;
        }

        public async Task<List<ListStreamKeyItemsResponse>> GetStreamKeyItems(string stream, string key)
        {
            var res = await RpcClient.ListStreamKeyItems(stream, key);
            return res.Result;
        }

        public async Task<string> GetNewWalletAddress()
        {
            var res = await RpcClient.GetNewAddressAsync();
            return res.Result;
        }

        public async Task<List<TransactionDetailsResponse>> GetWalletTransactions()
        {
            var res = await RpcClient.ListWalletTransactions();
            return res.Result;
        }

        public async Task<List<TransactionDetailsResponse>> GetAddressTransactions(string address)
        {
            var res = await RpcClient.ListAddressTransactionsAsync(address);
            return res.Result;
        }

        public async Task<string> IssueVote(string to)
        {
            var assets = await RpcClient.ListAssetsAsync();
            if (assets.Result.Any(a => a.Name == MultiChainTools.VOTE_ASSET_NAME))
            {
                var res = await RpcClient.IssueMoreAsync(to, MultiChainTools.VOTE_ASSET_NAME, 1);
                return res.Result;
            }
            else
            {
                var assetParams = new
                {
                    name = MultiChainTools.VOTE_ASSET_NAME,
                    open = true
                };
                var res = await RpcClient.IssueAsync(to, assetParams, 1, 1);
                return res.Result;
            }
        }

        public async Task<List<BlockchainQuestionModel>> GetQuestions()
        {
            // TODO: Handle multiple questions. For now assume exactly 1
            var result = await GetStreamKeyItems(MultiChainTools.ROOT_STREAM_NAME, MultiChainTools.QUESTIONS_KEY);
            var hex = result.First().Data;

            var bytes = MultiChainClient.ParseHexString(hex);
            var text = Encoding.UTF8.GetString(bytes);
            return new List<BlockchainQuestionModel>
            {
                JsonConvert.DeserializeObject<BlockchainQuestionModel>(text)
            };
        }

        #endregion
    }
}