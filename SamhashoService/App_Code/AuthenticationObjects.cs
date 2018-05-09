
namespace SamhashoService
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SignInUser
    {
        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public string Password { set; get; }

        [DataMember]
        public bool RememberMe { set; get; }
    }

    public class LoginRequest
    {
        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public string Password { set; get; }

    }

    public class RegisterRequest
    {
        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public string Password { set; get; }

    }

    [DataContract]
    public class ChangePassword
    {
        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public string NewPassword { set; get; }

        [DataMember]
        public string OldPassword { set; get; }
    }

    public enum UserRole
    {
        Administrator = 1,
        User
    }

    [DataContract]
    public class SignUp
    {
        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public UserRole UserRole { set; get; }

        [DataMember]
        public string Email { set; get; }

        [DataMember]
        public string PhoneNumber { set; get; }
    }

    public enum EmailTemplateId
    {
        Invitation = 1,
        Rating,
        SignUp,
        ResetPassword
    }
}