using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Blockchain.Models
{
    public class BlockchainVoteModelEncrypted
    {
        [DataMember(Name="magicWords")]
        public string MagicWords { get; set; }

        [DataMember(Name = "answers")]
        public string Answers { get; set; }

        public BlockchainVoteModelPlainText Decrypt(string keyStr)
        {
            // Get key for decryption from string
            var key = RsaTools.KeyPairFromString(keyStr);
            // Decrypt answers
            var plainAnswersStr = RsaTools.DecryptMessage(Answers, key.Private);
            // Read into answer models
            var answers = JsonConvert.DeserializeObject<List<BlockchainVoteAnswerModel>>(plainAnswersStr);

            // Convert to regular model
            return new BlockchainVoteModelPlainText
            {
                MagicWords = MagicWords,
                Answers = answers
            };
        }
    }
}