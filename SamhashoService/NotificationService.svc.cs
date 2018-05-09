
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SamhashoService.Extensions;
    using SamhashoService.Model;

    public class NotificationService : INotificationService
    {
        public ActionResult AddNotification(NotificationData notificationRequest)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    Notification notification = new Notification
                    {
                        IsDeleted = notificationRequest.IsDeleted,
                        DateCreated = DateTime.Now,
                        FromCustomer=notificationRequest.FromCustomer,
                        NotificationTypeId = notificationRequest.NotificationTypeId,
                        Email = notificationRequest.Email,
                        IsRead = notificationRequest.IsRead,
                        Message = notificationRequest.Message,
                        Name = notificationRequest.Name,
                        Phone = notificationRequest.Phone,
                        ProductId = notificationRequest.ProductId==0?null: notificationRequest.ProductId
                    };
                    entities.Notifications.Add(notification);
                    entities.SaveChanges();
                    return new ActionResult { Success = true, Message = "Notification saved successfully" };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = notificationRequest.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Notification);
                return new ActionResult { Message = "Error, notification failed to save, try again." };
            }
        }

        public List<NotificationData> GetAllNotifications()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<Notification> notificationList = entities.Notifications.Where(a => !a.IsDeleted).OrderByDescending(a=>a.DateCreated).ToList();
                    List<NotificationData > notifications= notificationList
                        .Select(a => new NotificationData{
                        Message = a.Message,
                        IsDeleted = a.IsDeleted,
                        Name = a.Name,
                        ProductId = a.ProductId,
                        IsRead = a.IsRead,
                        Phone = a.Phone,
                        DateCreated = a.DateCreated.ToLongDateString() + " " + a.DateCreated.ToLongTimeString(),
                        Email = a.Email,
                        Id = a.Id,
                        NotificationTypeId = a.NotificationTypeId,
                        FromCustomer = a.FromCustomer
                    }).ToList();
                    return notifications;
                }
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Notification);
                return new List<NotificationData>();
            }
        }

        public List<NotificationData> GetNotificationsByProduct(string productId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (!long.TryParse(productId, out long id))
                    {
                        return new List<NotificationData>();
                    }
                    List<Notification> notifications = entities.Notifications
                        .Where(a => !a.IsDeleted && a.ProductId == id).OrderByDescending(a=>a.DateCreated).ToList();
                    List<NotificationData> notificationDatas = notifications
                        .Select(
                            a => new NotificationData
                                     {
                                         Message = a.Message,
                                         IsDeleted = a.IsDeleted,
                                         Name = a.Name,
                                         ProductId = a.ProductId,
                                         IsRead = a.IsRead,
                                         Phone = a.Phone,
                                         DateCreated =
                                             a.DateCreated.ToLongDateString() + " " + a.DateCreated
                                                 .ToLongTimeString(),
                                         Email = a.Email,
                                         Id = a.Id
                                     }).ToList();
                    return notificationDatas;
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}

                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Notification);
                return new List<NotificationData>();
            }
        }

        public ActionResult ReadNotification(string notificationId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (!long.TryParse(notificationId, out long id))
                    {
                        return new ActionResult { Message = "Notification is invalid" };
                    }
                    Notification notification = entities.Notifications.FirstOrDefault(a => a.Id == id);
                    if (notification == null) return new ActionResult { Message = "Notification does not exist" };
                    notification.IsRead = true;
                    entities.SaveChanges();
                    return new ActionResult { Success = true, Message = "Notification saved successfully" };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"notificationId",notificationId}

                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Notification);
                return new ActionResult { Message = "Error, failed to mark notification as read" };
            }
        }
    }
}
