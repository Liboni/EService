using System;

namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserProfileDetails
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
       [DataMember]
        public string Gender { get; set; }
        [DataMember]
        public string ProfilePicture { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string AboutMe { get; set; }
        [DataMember]
        public string DateCreated { get; set; }
    }
}