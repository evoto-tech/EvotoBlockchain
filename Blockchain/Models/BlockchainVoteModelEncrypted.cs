using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Blockchain.Models
{
    public class BlockchainVoteModelEncrypted
    {
        [DataMember(Name = "answers")]
        public string Answers { get; set; }

        public BlockchainVoteModelPlainText Decrypt(string keyStr)
        {
            var key = RsaTools.KeyPairFromString(keyStr);
            var plainAnswersStr = RsaTools.DecryptMessage(Answers, key.Private);
            var answers = JsonConvert.DeserializeObject<List<BlockchainVoteAnswerModel>>(plainAnswersStr);
            return new BlockchainVoteModelPlainText
            {
                Answers = answers
            };
        }
    }
}