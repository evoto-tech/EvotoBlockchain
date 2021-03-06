﻿using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class BlockchainAnswerModel
    {
        [DataMember(Name = "answer")]
        public string Answer { get; set; }

        [DataMember(Name = "info")]
        public string Info { get; set; }
    }
}