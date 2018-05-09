
namespace SamhashoService.IdentityModels
{
    using System;
    using System.Drawing;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.DataProtection;

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
       
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

      public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            ApplicationUserManager manager =
                new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
                                        {
                                            AllowOnlyAlphanumericUserNames = true,
                                            RequireUniqueEmail = true
                                        };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
                                            {
                                                RequiredLength = 6,
                                                RequireNonLetterOrDigit = true,
                                                RequireDigit = true,
                                                RequireLowercase = true,
                                                RequireUppercase = true
                                            };

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider(
                "Phone Code",
                new PhoneNumberTokenProvider<ApplicationUser> { MessageFormat = "Your security code is {0}" });
            manager.RegisterTwoFactorProvider(
                "Email Code",
                new EmailTokenProvider<ApplicationUser>
                    {
                        Subject = "Security Code",
                        BodyFormat = "Your security code is {0}"
                    });

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10);
            manager.MaxFailedAccessAttemptsBeforeLockout = 4;

            manager.EmailService = new EmailService();
            IDataProtectionProvider dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }

        /// <summary>
        /// The draw text.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="font">
        /// The font.
        /// </param>
        /// <param name="textColor">
        /// The text color.
        /// </param>
        /// <param name="backColor">
        /// The back color.
        /// </param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        public static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            var img = new Bitmap(1, 1);
            var drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            var textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            var width = Math.Max((int)textSize.Width, (int)textSize.Height);

            //create a new image of the right size, make it square using the larger of the 2 sides
            img = new Bitmap(width, width);

            var xOffset = (width - (int)textSize.Width) / 2;
            var yOffset = (width - (int)textSize.Height) / 2;

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            using (var textBrush = new SolidBrush(textColor))
            {
                drawing.DrawString(text, font, textBrush, xOffset, yOffset);

                drawing.Save();
                drawing.Dispose();
            }

            return img;

        }

    }
}