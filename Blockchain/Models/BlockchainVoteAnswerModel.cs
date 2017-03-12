using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainVoteAnswerModel
    {
        [DataMember(Name = "num")]
        public int Number { get; set; }

        [DataMember(Name = "question")]
        public int Question { get; set; }

        [DataMember(Name = "answer")]
        public string Answer { get; set; }
    }
}