
namespace SamhashoService
{
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IProductService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/CreateProduct")]
        CreateProductResponse CreateProduct(Stream product);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/SearchProduct")]
        List<ProductResponse> SearchProduct(SearchProductRequest searchProductRequest);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/DeleteProduct/{productId}")]
        ActionResult DeleteProduct(string productId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/DeleteProductMedia/{productId}")]
        ActionResult DeleteProductMedia(string productId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddProductMedia")]
        ProductMediaResponse AddProductMedia(Stream product);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetProduct/{productId}")]
        ProductResponse GetProduct(string productId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllProducts")]
        List<ProductResponse> GetAllProducts();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/UpdateProduct")]
        ActionResult UpdateProduct(ProductRequest product);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/CreateGoogleDriveFolder/{folderName}")]
        string CreateGoogleDriveFolder(string folderName);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetProductMedia/{productId}")]
        List<ProductMediaResponse> GetProductMedia(string productId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddProductViewer")]
        ActionResult AddProductViewer(Viewer request);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetProductViews")]
        List<ViewerResponse> GetProductViews();
    }

}
