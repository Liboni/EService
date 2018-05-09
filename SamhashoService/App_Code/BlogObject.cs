
namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class BlogResponse
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string TitleDescription { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public string MediaUrl { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string DateCreated { get; set; }
    }
}