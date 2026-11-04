using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JiraClone_EF6.Web.Startup))]
namespace JiraClone_EF6.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
