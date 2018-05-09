using System;


namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Viewer
    {
        [DataMember]
        public int ViewId { get; set; }
        [DataMember]
        public long DataId { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Town { get; set; }
        [DataMember]
        public string IpAddress { get; set; }
    }

    [DataContract]
    public class ViewerResponse
    {
        [DataMember]
        public int ViewId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Town { get; set; }
        [DataMember]
        public string IpAddress { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }
    }
}