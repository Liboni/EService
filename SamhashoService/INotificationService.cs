
namespace SamhashoService
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface INotificationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/AddNotification")]
        ActionResult AddNotification(NotificationData notificationRequest);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllNotifications")]
        List<NotificationData> GetAllNotifications();

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetNotificationsByProduct/{productId}")]
        List<NotificationData> GetNotificationsByProduct(string productId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/ReadNotification/{notificationId}")]
        ActionResult ReadNotification(string notificationId);
    }
}
