

namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class NotificationData
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public bool FromCustomer { get; set; }

        [DataMember]
        public byte NotificationTypeId { get; set; }

        [DataMember]
        public long? ProductId { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public bool IsRead { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string DateCreated { get; set; }
    }

}