
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Web;
    using HttpMultipartParser;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using SamhashoService.Extensions;
    using SamhashoService.IdentityModels;
    using SamhashoService.Model;

    public class UserService : IUserService
    {
       public UserProfileDetails GetUserDetails(Token token)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    UserProfile userDetails = entities.UserProfiles.FirstOrDefault();
                    if (userDetails != null)
                    {
                        ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        ApplicationUser aspUser = userManager.FindById(userDetails.UserId);
                        UserProfileDetails user = new UserProfileDetails
                        {

                            DateCreated = userDetails.DateCreated.ToLongDateString(),
                            Id = userDetails.Id,
                            UserId = userDetails.UserId,
                            City = userDetails.City,
                            AboutMe = userDetails.AboutMe,
                            Address = userDetails.Address,
                            FirstName = userDetails.FirstName,
                            LastName = userDetails.LastName,
                            ProfilePicture = userDetails.ProfilePicture,
                            Country = userDetails.Country,
                            UserName = aspUser.UserName,
                            Email = aspUser.Email,
                            PhoneNumber= aspUser.PhoneNumber
                        };
                        return user;
                    }

                    return new UserProfileDetails();
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = token.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.User);
                return new UserProfileDetails();
            }
        }

       public ActionResult UpdateUserDetails(Stream userData)
        {
            MultipartFormDataParser dataParser = new MultipartFormDataParser(userData);
            try
            {
                string aboutMe = dataParser.GetParameterValue("aboutMe");
                string address = dataParser.GetParameterValue("address");
                string city = dataParser.GetParameterValue("city");
                string country = dataParser.GetParameterValue("country");
                string firstName = dataParser.GetParameterValue("firstName");
                string lastName = dataParser.GetParameterValue("lastName");
                string token = dataParser.GetParameterValue("token");
                string phoneNumber = dataParser.GetParameterValue("phonenumber");
                string email = dataParser.GetParameterValue("email");
               
                AuthenticationService authenticationService = new AuthenticationService();
                IPrincipal jwtToken =authenticationService.AuthenticateJwtToken(token);
                string userId = jwtToken.Identity.GetUserId();
                string path = null;
                if (dataParser.Files.Any())
                {
                    FilePart dataParserFile = dataParser.Files[0];
                    path = ServiceHelper.SaveImage(dataParserFile.Data, dataParserFile.FileName);
                }
                
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    AspNetUser netUser = entities.AspNetUsers.FirstOrDefault(a => a.Id.Equals(userId));
                    if (netUser != null) {
                        netUser.Email = String.IsNullOrEmpty(email) ? netUser.Email : email;
                        netUser.PhoneNumber = String.IsNullOrEmpty(phoneNumber) ? netUser.PhoneNumber : phoneNumber;
                    }
                    UserProfile userProfile = entities.UserProfiles.FirstOrDefault(a=>a.UserId.Equals(userId));
                    if (userProfile != null)
                    {
                        userProfile.AboutMe = String.IsNullOrEmpty(aboutMe) ? userProfile.AboutMe : aboutMe;
                        userProfile.Address = String.IsNullOrEmpty(address) ? userProfile.Address : address;
                        userProfile.City = String.IsNullOrEmpty(city) ? userProfile.City : city;
                        userProfile.Country = String.IsNullOrEmpty(country) ? userProfile.Country : country;
                        userProfile.FirstName = String.IsNullOrEmpty(firstName) ? userProfile.FirstName : firstName;
                        userProfile.LastName = String.IsNullOrEmpty(lastName) ? userProfile.LastName : lastName;
                        userProfile.ProfilePicture = String.IsNullOrEmpty(path)?userProfile.ProfilePicture:path;
                        userProfile.UserId = userId;
                    }
                    else
                    {
                        UserProfile addUserProfile = new UserProfile
                                                      {
                                                          DateCreated = DateTime.Now,
                                                          AboutMe = aboutMe,
                                                          Address = address,
                                                          City = city,
                                                          Country = country,
                                                          FirstName = firstName,
                                                          LastName = lastName,
                                                          ProfilePicture = path,
                                                          UserId = userId
                                                      };
                        entities.UserProfiles.Add(addUserProfile);
                    }
                    entities.SaveChanges();
                    return new ActionResult
                               {
                                   Message = path,
                                   Success = true
                               };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"aboutMe",dataParser.GetParameterValue("aboutMe")},
                                                                {"address", dataParser.GetParameterValue("address")},
                                                                {"city", dataParser.GetParameterValue("city")},
                                                                {"country", dataParser.GetParameterValue("country")},
                                                                {"firstName", dataParser.GetParameterValue("firstName")},
                                                                {"lastName", dataParser.GetParameterValue("lastName")},
                                                                {"userId", dataParser.GetParameterValue("userId")}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.User);
                return new ActionResult
                           {
                               Message = "Failed to save profile, try again.",
                               Success = true
                           };
            }
        }
    }
}
