
namespace SamhashoService.IdentityModels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;

    using SamhashoService.Extensions;

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                MailMessage emailMessage = new MailMessage
                                                  {
                                                      Sender = new MailAddress("noreply@gmail.com", "ESamhasho"),
                                                      From = new MailAddress("noreply@gmail.com", "ESamhasho"),
                                                      IsBodyHtml = true,
                                                      Subject = message.Subject,
                                                      Body = message.Body,
                                                      Priority = MailPriority.Normal
                                                  };
                emailMessage.To.Add(message.Destination);
                using (SmtpClient mailClient = new SmtpClient())
                {
                   mailClient.Send(emailMessage);
                }
                return Task.FromResult(EmailStatus.Success);
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = message.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.EmailService);
                return Task.FromResult(EmailStatus.Failed);
            }
        }
    }
}