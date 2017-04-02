using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainVoteModelPlainText
    {
        [DataMember(Name = "answers")]
        public List<BlockchainVoteAnswerModel> Answers { get; set; }
    }
}