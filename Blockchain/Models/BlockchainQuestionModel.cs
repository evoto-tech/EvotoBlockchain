using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainQuestionModel
    {
        [DataMember(Name = "question")]
        public string Question { get; set; }

        [DataMember(Name = "answers")]
        public List<string> Answers { get; set; }
    }
}