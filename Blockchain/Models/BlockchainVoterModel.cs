using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainVoterModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
    }
}