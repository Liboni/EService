
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;

    using SamhashoService.Extensions;
    using SamhashoService.IdentityModels;
    using SamhashoService.Model;

    public class UserManagement
    {
        public static VerificationToken GetVerificationToken(string guidCode)
        {
            try
            {
                ESamhashoEntities entities = new ESamhashoEntities();
                return entities.VerificationTokens.FirstOrDefault(a => a.Id.Equals(guidCode));
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = guidCode.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.UserManagement);
                return new VerificationToken();
            }
        }

        public static ActionResult SaveVerificationDetails(string email, string token)
        {
            VerificationToken verification = new VerificationToken();
            try
            {
                ESamhashoEntities entities = new ESamhashoEntities();
                string guidCode = Guid.NewGuid().ToString();
                verification = new VerificationToken
                                   {
                                       Id = guidCode,
                                       ExpiryDate = DateTime.Now.AddDays(1),
                                       UserEmail = email,
                                       UserToken = token
                                   };
                entities.VerificationTokens.Add(verification);
                entities.SaveChanges();
                return new ActionResult
                           {
                               Success = true,
                               Message = verification.Id
                           };
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = verification.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.UserManagement);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to save verification details."
                           };
            }
        }

        public ActionResult SendPasswordResetEmail(string emailAddress, string passwordResetToken)
        {
            // var requestUrl = new Uri(HttpContext.Current.Request.Url, $"/#/passwordreset/{passwordResetToken}");
            // EmailPasswordReset msg = new EmailPasswordReset
            // {
            // verificationUrl = requestUrl.ToString()
            // };
            string json;// = new JavaScriptSerializer().Serialize(msg);
            using (ESamhashoEntities entities = new ESamhashoEntities())
            {

            }

            return new ActionResult
                       {
                           Success = true,
                           Message = "Email sent, please check your inbox to confirm"
                       };
        }

        public static ActionResult VerifyToken(string email, string token)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindByEmail(email);
            if (user == null)
                return new ActionResult
                           {
                               Message = "User not found. Ensure that you copy and paste the link sent to your email properly."
                           };

            IdentityResult result = userManager.ConfirmEmail(user.Id, token);
            return result.Succeeded ? new ActionResult { Message = "Verification successful", Success = true } : new ActionResult { Message = string.Join("\r\n", result.Errors) };
        }

    }
}