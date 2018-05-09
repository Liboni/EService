
namespace SamhashoService
{
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IBlogService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddBlog")]
        BlogResponse AddBlog(Stream blogData);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/DeleteBlog/{blogId}")]
        ActionResult DeleteBlog(string blogId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetBlogs")]
        List<BlogResponse> GetBlogs();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetBlogViews")]
        List<ViewerResponse> GetBlogViews();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddBlogViewer")]
        ActionResult AddBlogViewer(Viewer request);
    }
}
