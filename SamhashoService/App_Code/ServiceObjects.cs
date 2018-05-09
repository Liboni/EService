

namespace SamhashoService {
    using System.IdentityModel.Tokens.Jwt;
    using System.Runtime.Serialization;

    public enum MediaTypes
    {
        Image,
        Audio,
        Video,
        Document
    }

    public enum ErrorSource
    {
        Authentication = 1,
        Notification,
        Product,
        Catergory,
        EmailService,
        UserManagement,
        Blog,
        ServiceHelper,
        Security,
        User
    }

    public enum EmailStatus
    {
        Pending = 1,
        Success,
        Failed
    }

    [DataContract]
    public class ActionResult
    {
        [DataMember]
        public bool Success { set; get; }

        [DataMember]
        public string Message { set; get; }

    }

}