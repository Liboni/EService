
namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CatergoryRequest
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }

    }

    [DataContract]
    public class CatergoryResponse
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }

    }
}