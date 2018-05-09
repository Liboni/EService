
using Microsoft.Owin;

[assembly: OwinStartup(typeof(SamhashoService.Startup))]
namespace SamhashoService
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}