
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;
    using Google.Apis.Util.Store;

    public static class ServiceHelper
    {
        static readonly string[] Scopes = { DriveService.Scope.Drive };
        
        public static string SaveImage(Stream image, string name)
        {
            try
            {
                const string Type = "image/jpeg";
                string folderId = WebConfigurationManager.AppSettings["GoogleDriveFolderId"];
                Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = name,
                    MimeType = Type,
                    Parents = new List<string>{
                                                folderId
                                              }
                };
                UserCredential credential = GetCredentials();
                DriveService service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = WebConfigurationManager.AppSettings["GoogleDriveApplicationName"]
                });
                FilesResource.CreateMediaUpload request = service.Files.Create(fileMetadata, image, Type);
                request.Fields = "id";
                request.Upload();
                Google.Apis.Drive.v3.Data.File file = request.ResponseBody;
                string googleDriveUrl = "https://drive.google.com/uc?id=" + file.Id + "&export=stream";
                return googleDriveUrl;
            }
            catch (Exception exception)
            {
                LogException(exception,new Dictionary<string, string>(),ErrorSource.ServiceHelper );
                return String.Empty;
            }
        }

        public static string CreateFolder(string folderName)
        {
            try
            {
                UserCredential credential = GetCredentials();
                DriveService service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = WebConfigurationManager.AppSettings["GoogleDriveApplicationName"]
                });

                Google.Apis.Drive.v3.Data.File fileMetaData = new Google.Apis.Drive.v3.Data.File
                    {
                        Name = folderName,
                        MimeType = "application/vnd.google-apps.folder"
                    };

                FilesResource.CreateRequest request = service.Files.Create(fileMetaData);
                request.Fields = "id";
                Google.Apis.Drive.v3.Data.File file = request.Execute();
                return file.Id;
            }
            catch (Exception exception)
            {
                LogException(exception,new Dictionary<string, string>(),ErrorSource.ServiceHelper);
                return String.Empty;
            }
        }

        public static UserCredential GetCredentials()
        {
            UserCredential credential;
            string path = HostingEnvironment.MapPath("~\\App_Data\\");
            using (FileStream stream = new FileStream(path + "client_secret.json", FileMode.Open, FileAccess.Read))
            {
                path = Path.Combine(path, ".credentials/drive-dotnet-quickstart.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(path, true))
                    .Result;
            }

            return credential;
        }

        public static void LogException(Exception exception, Dictionary<string, string> data, ErrorSource source)
        {
            string folder = source.ToString();

            string filename = "Errors " + DateTime.Now.ToString("dd-MMM-yyyy") + ".txt";
            string path = HostingEnvironment.MapPath("~\\") + "\\Logs\\GeneralErrors\\" + folder + "\\"
                          + DateTime.Now.ToString("dd-MMM-yyyy");
            filename = filename.Replace("/", string.Empty);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string logText = "TimeStamp : " + "\t" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + Environment.NewLine
                             + "Error : " + "\t" + exception.Message + Environment.NewLine + "Source : " + "\t"
                             + exception.Source + Environment.NewLine + "Data : " + "\t" + data + "Stacktrace :" + "\t"
                             + exception.StackTrace + Environment.NewLine
                             + "===============================================================================================================================================";
            File.AppendAllText(path + "\\" + filename, logText);

            if (exception.InnerException != null)
            {
                string logText1 = "TimeStamp : " + "\t" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + Environment.NewLine
                                  + "Error : " + "\t" + exception.InnerException.Message + Environment.NewLine + "Source : " + "\t"
                                  + exception.InnerException.Source + Environment.NewLine + "Data : " + "\t" + data + "Stacktrace :" + "\t"
                                  + exception.InnerException.StackTrace + Environment.NewLine
                                  + "===============================================================================================================================================";
                File.AppendAllText(path + "\\" + filename, logText1);
            }
        }
        
        public static ActionResult IsAnyNullOrEmpty(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return new ActionResult
                               {
                                   Success = true,
                                   Message = "field is required."
                               };
                }
                foreach (PropertyInfo pi in obj.GetType().GetProperties())
                {
                    dynamic value;
                    bool result;
                    if (pi.PropertyType == typeof(string))
                    {
                        value = (string)pi.GetValue(obj);
                        result = string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
                        if (result)
                            return new ActionResult
                                       {
                                           Success = true,
                                           Message = AddSpacesToSentence(pi.Name, false) + " is required."
                                       };
                    }
                    if (pi.PropertyType == typeof(int))
                    {
                        value = (int)pi.GetValue(obj);
                        result = value <= 0 || value == null;
                        if (result)
                            return new ActionResult
                                       {
                                           Success = true,
                                           Message = AddSpacesToSentence(pi.Name, false) + " is required."
                                       };
                    }
                    if (pi.PropertyType == typeof(bool))
                    {
                        value = pi.GetValue(obj);
                        result = value == null;
                        if (result)
                            return new ActionResult
                                       {
                                           Success = true,
                                           Message = AddSpacesToSentence(pi.Name, false) + " is required."
                                       };
                    }
                    if (pi.PropertyType == typeof(Guid))
                    {

                        value = pi.GetValue(obj);
                        result = value == Guid.Empty || value == null;
                        if (result)
                            return new ActionResult
                                       {
                                           Success = true,
                                           Message = AddSpacesToSentence(pi.Name, true) + " is required."
                                       };
                    }
                }
                return new ActionResult { Success = false };

            }
            catch (Exception)
            {
                return new ActionResult
                           {
                               Success = true,
                               Message = "field is required."
                           };

            }
        }

        public static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) ||
                        preserveAcronyms && char.IsUpper(text[i - 1]) &&
                        i < text.Length - 1 && !char.IsUpper(text[i + 1]))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static ActionResult CheckAuthentication()
        {
            IIdentity identity = HttpContext.Current.User.Identity;
            if (identity.IsAuthenticated) return new ActionResult { Success = true, Message = "" };
            //string returnUrl = string.IsNullOrEmpty(HttpContext.Current.Request.Url.PathAndQuery)
            //                       ? string.Empty
            //                       : WebUtility.UrlEncode(HttpContext.Current.Request.Url.PathAndQuery);
            //HttpContext.Current.Response.Redirect(@"/index.html" + (string.IsNullOrEmpty(returnUrl) ? string.Empty : string.Format("?ReturnUrl={0}", returnUrl)));
            return new ActionResult { Success = false, Message = "Please login." };
        }

    }
}