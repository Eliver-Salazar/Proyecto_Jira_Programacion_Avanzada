using System.Web.Mvc;
using JiraClone_EF6.Web.Filters;

namespace JiraClone_EF6.Web
{
    /// <summary>
    /// Registra los filtros compartidos por los controladores MVC.
    /// </summary>
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Conserva el manejo de errores existente.
            filters.Add(new HandleErrorAttribute());

            // Aplica la política de caché a las respuestas autenticadas.
            filters.Add(new NoStoreAuthenticatedAttribute());
        }
    }
}