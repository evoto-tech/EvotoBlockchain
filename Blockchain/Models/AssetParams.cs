using System.Runtime.Serialization;

namespace Blockchain.Models
{
    public class AssetParams
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "open")]
        public bool Open { get; set; }
    }
}