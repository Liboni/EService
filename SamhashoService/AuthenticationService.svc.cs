
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Security;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;

    using SamhashoService.Extensions;
    using SamhashoService.IdentityModels;
    using SamhashoService.Model;

      public class AuthenticationService : IAuthenticationService
    {
        public ActionResult SignIn(SignInUser signInUser)
        {
            try
            {
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(signInUser);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                ApplicationSignInManager signinManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationSignInManager>();

                SignInStatus result = signinManager.PasswordSignIn(signInUser.Username, signInUser.Password, signInUser.RememberMe, false);
                switch (result)
                {
                    case SignInStatus.Success:

                        ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        ApplicationUser user = userManager.Find(signInUser.Username, signInUser.Password);
                        if (!userManager.IsEmailConfirmed(user.Id))
                        {
                            return new ActionResult { Success = false, Message = "You need to confirm your email." };
                        }

                        IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                        ClaimsIdentity userIdentity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                        authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = signInUser.RememberMe }, userIdentity);
                        string token = Security.GenerateSecurityToken(user.Id, signInUser.Username, user.Email);
                        return new ActionResult
                                   {
                                       Success = true,
                                       Message = token
                                   };
                    case SignInStatus.LockedOut:
                        userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        user = userManager.FindByName(signInUser.Username);
                        user.LockoutEnabled = true;
                        userManager.Update(user);
                        return new ActionResult
                                   {
                                       Success = false,
                                       Message = "Your account has been locked out, please contact your system administrator."
                                   };
                    case SignInStatus.RequiresVerification:
                        return new ActionResult
                                   {
                                       Success = false,
                                       Message = "Your account has not yet been verified, please contact your system administrator."
                                   };
                    case SignInStatus.Failure:
                        ApplicationUserManager manager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        ApplicationUser tryUser = manager.FindByName(signInUser.Username);
                        if (tryUser != null)
                        {
                            manager.AccessFailed(tryUser.Id);
                        }

                        return new ActionResult
                                   {
                                       Success = false,
                                       Message = "Invalid username/password combination."
                                   };
                    default:
                        return new ActionResult
                                   {
                                       Success = false,
                                       Message = "Could not login\r\n" + result
                                   };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = signInUser.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, Failed to login."
                           };
            }
        }

        public ActionResult VerifyEmail(string guidCode)
        {
            try
            {
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(guidCode);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                VerificationToken verification = UserManagement.GetVerificationToken(guidCode);
                ActionResult result = UserManagement.VerifyToken(verification.UserEmail, verification.UserToken);
                Uri requestUrl = new Uri(HttpContext.Current.Request.Url, "/index.html?" + result.Success);
                HttpContext.Current.Response.Redirect(requestUrl.ToString());
                return result;
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = guidCode.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to verify email."
                           };
            }
        }

        public ActionResult SignUp(SignUp signUp)
        {
            try
            {
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(signUp);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                ApplicationUserManager manager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string password = PasswordService.GeneratePassword(PasswordOptions.HasCapitals |
                                                                   PasswordOptions.HasDigits |
                                                                   PasswordOptions.HasLower |
                                                                   PasswordOptions.HasSymbols |
                                                                   PasswordOptions.NoRepeating);
                Guid userId = Guid.NewGuid();
                manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10);
                manager.MaxFailedAccessAttemptsBeforeLockout = 3;
                manager.UserLockoutEnabledByDefault = true;
                ApplicationUser applicationUser = new ApplicationUser
                                                      {
                                                          UserName = signUp.Username,
                                                          Email = signUp.Email,
                                                          PhoneNumber = signUp.PhoneNumber,
                                                          Id = userId.ToString(),
                                                          LockoutEnabled = true
                                                      };
                IdentityResult result = manager.Create(applicationUser, password);
                string error = result.Errors.Aggregate(string.Empty, (current, resultError) => current + resultError + Environment.NewLine);
                if (!result.Succeeded)
                {
                    return new ActionResult
                               {
                                   Success = false,
                                   Message = error
                               };
                }

                string userRole = signUp.UserRole.ToString();
                result = manager.AddToRole(applicationUser.Id, userRole);
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    string emailConfirmationToken = manager.GenerateEmailConfirmationToken(applicationUser.Id);
                    ActionResult guid = UserManagement.SaveVerificationDetails(applicationUser.Email, emailConfirmationToken);
                    if (!guid.Success)
                    {
                        return new ActionResult
                                   {
                                       Success = false,
                                       Message = "Failed to register new user."
                                   };
                    }

                    Uri requestUrl = new Uri(HttpContext.Current.Request.Url, "/Backoffice/Authentication.svc/json/VerifyEmail/" + guid.Message);
                    EmailTemplate emailBody = entities.EmailTemplates.FirstOrDefault(a => a.EmailTemplateId == (byte)EmailTemplateId.SignUp);
                    if (emailBody != null)
                    {
                        string body = emailBody.Body.Replace("{activateaccount}", requestUrl.ToString())
                            .Replace("{email}", applicationUser.Email)
                            .Replace("{password}", password)
                            .Replace("{accountname}", applicationUser.UserName);
                        string subject = "Esamhasho registration confirmation";
                        OutgoingEmail outgoingEmail = new OutgoingEmail
                                                          {
                                                              Body = body,
                                                              Date = DateTime.Now,
                                                              Destination = applicationUser.Email,
                                                              Reference = applicationUser.Id,
                                                              Status = (byte)EmailStatus.Pending,
                                                              Subject = subject
                                                          };
                        entities.OutgoingEmails.Add(outgoingEmail);
                        entities.SaveChanges();
                        manager.SendEmail(applicationUser.Id, subject, body);
                        OutgoingEmail email = entities.OutgoingEmails.FirstOrDefault(a => a.Id == outgoingEmail.Id);
                        if (email != null) email.Status = (byte)EmailStatus.Success;
                        entities.SaveChanges();
                    }
                }

                if (result.Succeeded)
                {
                    return new ActionResult
                               {
                                   Success = true,
                                   Message = "Successfully register new user. The user should check there email to avtivate there account."
                               };
                }

                error = result.Errors.Aggregate(string.Empty, (current, resultError) => current + resultError + Environment.NewLine);
                return new ActionResult
                           {
                               Success = false,
                               Message = error
                           };

            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = signUp.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to register new user."
                           };
            }
        }

        public ActionResult ForgotPassword(string username)
        {
            try
            {
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(username);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                ApplicationUser user = manager.FindByEmail(username);
                if (user == null)
                {
                    return new ActionResult
                               {
                                   Message = "User does not exist",
                                   Success = false
                               };
                }

                if (!manager.IsEmailConfirmed(user.Id))
                    return new ActionResult
                               {
                                   Success = false,
                                   Message = "You need to confirm your email."
                               };
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string resetToken = userManager.GeneratePasswordResetToken(user.Id);
                string newPassword = PasswordService.GeneratePassword(PasswordOptions.HasCapitals |
                                                                      PasswordOptions.HasDigits |
                                                                      PasswordOptions.HasLower |
                                                                      PasswordOptions.HasSymbols |
                                                                      PasswordOptions.NoRepeating);

                IdentityResult result = userManager.ResetPassword(user.Id, resetToken, newPassword);
                if (result.Succeeded)
                {
                    using (ESamhashoEntities entities = new ESamhashoEntities())
                    {
                        EmailTemplate emailBody = entities.EmailTemplates.FirstOrDefault(a => a.EmailTemplateId == (byte)EmailTemplateId.ResetPassword);
                        if (emailBody != null)
                        {
                            string body = emailBody.Body.Replace("{password}", newPassword)
                                .Replace("{accountname}", user.UserName);
                            string subject = "Esamhasho password reset";
                            OutgoingEmail outgoingEmail = new OutgoingEmail
                                                              {
                                                                  Body = body,
                                                                  Date = DateTime.Now,
                                                                  Destination = user.Email,
                                                                  Reference = user.Id,
                                                                  Status = (byte)EmailStatus.Pending,
                                                                  Subject = subject
                                                              };
                            entities.OutgoingEmails.Add(outgoingEmail);
                            entities.SaveChanges();
                            manager.SendEmail(user.Id, subject, body);
                            OutgoingEmail email = entities.OutgoingEmails.FirstOrDefault(a => a.Id == outgoingEmail.Id);
                            if (email != null) email.Status = (byte)EmailStatus.Success;
                            entities.SaveChanges();
                        }
                    }

                    return new ActionResult
                               {
                                   Message = "Password successfully reset. Check your email for your new password.",
                                   Success = true
                               };
                }

                string error = result.Errors.Aggregate(string.Empty, (current, resultError) => current + resultError + Environment.NewLine);
                return new ActionResult { Message = error, Success = result.Succeeded };
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = username.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to reset password."
                           };
            }


        }

        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            try
            {
                string token = HttpContext.Current.Request.Headers["Token"];
                if (!Security.ValidateToken(token, out List<Claim> claims))
                {
                    return new ActionResult
                               {
                                   Message = "Please login",
                                   Success = false
                               };
                }
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(changePassword);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                ApplicationUser user = manager.FindByName(changePassword.Username);
                if (user == null)
                {
                    return new ActionResult
                               {
                                   Message = "User does not exist",
                                   Success = false
                               };
                }

                IdentityResult result = manager.ChangePassword(user.Id, changePassword.OldPassword, changePassword.NewPassword);
                if (result.Succeeded)
                {
                    using (ESamhashoEntities entities = new ESamhashoEntities())
                    {
                        EmailTemplate emailBody = entities.EmailTemplates.FirstOrDefault(a => a.EmailTemplateId == (byte)EmailTemplateId.ResetPassword);
                        if (emailBody != null)
                        {
                            string body = emailBody.Body.Replace("{password}", changePassword.NewPassword)
                                .Replace("{accountname}", user.UserName);
                            string subject = "Esamhasho password reset";
                            OutgoingEmail outgoingEmail = new OutgoingEmail
                                                              {
                                                                  Body = body,
                                                                  Date = DateTime.Now,
                                                                  Destination = user.Email,
                                                                  Reference = user.Id,
                                                                  Status = (byte)EmailStatus.Pending,
                                                                  Subject = subject
                                                              };
                            entities.OutgoingEmails.Add(outgoingEmail);
                            entities.SaveChanges();
                            manager.SendEmail(user.Id, subject, body);
                            OutgoingEmail email = entities.OutgoingEmails.FirstOrDefault(a => a.Id == outgoingEmail.Id);
                            if (email != null) email.Status = (byte)EmailStatus.Success;
                            entities.SaveChanges();
                        }
                    }

                    return new ActionResult
                               {
                                   Message = "Password successfully changed",
                                   Success = true
                               };
                }

                string error = result.Errors.Aggregate(string.Empty, (current, resultError) => current + resultError + Environment.NewLine);
                return new ActionResult
                           {
                               Message = error
                           };
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = changePassword.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to change password."
                           };
            }
        }

        public ActionResult LockUser(string username)
        {
            try
            {
                ActionResult actionResult = ServiceHelper.IsAnyNullOrEmpty(username);
                if (actionResult.Success)
                {
                    actionResult.Success = false;
                    return actionResult;
                }

                ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                ApplicationUser user = manager.FindByName(username);
                if (user == null)
                {
                    return new ActionResult
                               {
                                   Message = "User does not exist",
                                   Success = false
                               };
                }

                IdentityResult result = manager.SetLockoutEnabled(user.Id, true);
                if (result.Succeeded)
                {
                    return new ActionResult
                               {
                                   Message = "User successfully locked.",
                                   Success = true
                               };
                }

                string error = result.Errors.Aggregate(string.Empty, (current, resultError) => current + resultError + Environment.NewLine);
                return new ActionResult
                           {
                               Message = error
                           };
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = username.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to lock user."
                           };
            }

        }

        public ActionResult GetAllUsers()
        {
            try
            {
                ActionResult authentication = ServiceHelper.CheckAuthentication();
                if (!authentication.Success) return authentication;

                // var membershipUserCollection = Membership.GetAllUsers();
                // string json = new JavaScriptSerializer().Serialize(membershipUserCollection);
                return new ActionResult
                           {
                               Message = string.Empty,
                               Success = true,

                               // DataObjects = json
                           };
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to get all users."
                           };
            }
        }

        public ActionResult GetNumberOfUserOnline()
        {
            try
            {
                ActionResult authentication = ServiceHelper.CheckAuthentication();
                if (!authentication.Success) return authentication;
                int numberOfUsersOnline = Membership.GetNumberOfUsersOnline();
                return new ActionResult
                           {
                               Message = string.Empty,
                               Success = true,

                               // DataObjects = numberOfUsersOnline.ToString()
                           };
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Authentication);
                return new ActionResult
                           {
                               Success = false,
                               Message = "Error, failed to get number of users online."
                           };
            }
        }

        public IPrincipal AuthenticateJwtToken(string token)
        {
            if (Security.ValidateToken(token, out List<Claim> claims))
            {
                ClaimsIdentity identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);
                return user;
            }
            return null;
        }
    }
}
