using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainVoteAnswerModel
    {
        [DataMember(Name = "question")]
        public int Question { get; set; }

        [DataMember(Name = "answer")]
        public string Answer { get; set; }
    }
}