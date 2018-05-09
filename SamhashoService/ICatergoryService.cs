
namespace SamhashoService
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface ICatergoryService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddCatergory/{catergoryName}")]
        ActionResult AddCatergory(string catergoryName);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllCatergories")]
        List<CatergoryResponse> GetAllCatergories();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/DeleteCatergory/{catergoryId}")]
        ActionResult DeleteCatergory(string catergoryId);
    }
}
