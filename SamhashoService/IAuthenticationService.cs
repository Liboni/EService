
namespace SamhashoService
{
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/SignIn")]
        ActionResult SignIn(SignInUser signInUser);

        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AuthenticateJwtToken/{token}")]
        IPrincipal AuthenticateJwtToken(string token);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/SignUp")]
        ActionResult SignUp(SignUp signUp);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/ForgotPassword/{username}")]
        ActionResult ForgotPassword(string username);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/ChangePassword")]
        ActionResult ChangePassword(ChangePassword changePassword);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/LockUser/{username}")]
        ActionResult LockUser(string username);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/VerifyEmail/{guidCode}")]
        ActionResult VerifyEmail(string guidCode);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllUsers")]
        ActionResult GetAllUsers();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetNumberOfUserOnline")]
        ActionResult GetNumberOfUserOnline();
    }
}
