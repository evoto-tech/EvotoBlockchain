using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Blockchain.Models
{
    public class BlockchainVoteModelPlainText
    {
        [DataMember(Name = "magicWords")]
        public string MagicWords { get; set; }

        [DataMember(Name = "answers")]
        public List<BlockchainVoteAnswerModel> Answers { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Answers);
        }
    }
}