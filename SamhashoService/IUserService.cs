
namespace SamhashoService
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetUserDetails")]
        UserProfileDetails GetUserDetails(Token token);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/UpdateUserDetails")]
        ActionResult UpdateUserDetails(Stream userData);
    }
}
