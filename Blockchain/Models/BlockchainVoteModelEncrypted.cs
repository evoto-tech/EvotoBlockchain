using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainVoteModelEncrypted : BlockchainVoteModelBase
    {
        [DataMember(Name = "answers")]
        public string Answers { get; set; }
    }
}