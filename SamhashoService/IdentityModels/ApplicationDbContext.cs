
namespace SamhashoService.IdentityModels
{
   using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The application database provider
    /// </summary>
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
       public ApplicationDbContext()
            : base("SecurityModuleConnectionString", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}