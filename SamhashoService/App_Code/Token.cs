
namespace SamhashoService
{
    using System.Runtime.Serialization;

    public class Token
    {
        [DataMember]
        public string Name { get; set; }
    }
}