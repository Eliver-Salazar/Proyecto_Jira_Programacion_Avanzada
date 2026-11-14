using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace JiraClone_EF6.Web
{
    /// <summary>
    /// Configura la aplicación al iniciarse.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Registra las dependencias de los controladores y repositorios.
            DependencyConfig.RegisterDependencies();

            // Registra las áreas funcionales.
            AreaRegistration.RegisterAllAreas();

            // Configura filtros, rutas y recursos de la interfaz.
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}