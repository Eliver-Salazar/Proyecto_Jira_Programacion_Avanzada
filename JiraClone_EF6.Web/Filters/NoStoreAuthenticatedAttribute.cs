using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

namespace JiraClone_EF6.Web.Filters
{
    /// <summary>
    /// Evita almacenar respuestas MVC asociadas a un usuario autenticado.
    /// No reemplaza la autorización del servidor.
    /// </summary>
    public sealed class NoStoreAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Las acciones hijas comparten la respuesta de la página principal.
            if (filterContext.IsChildAction)
            {
                return;
            }

            var user = filterContext.HttpContext.User;

            if (user == null || user.Identity == null
                || !user.Identity.IsAuthenticated)
            {
                return;
            }

            // Impide guardar la respuesta en cachés HTTP.
            var cache = filterContext.HttpContext.Response.Cache;

            cache.SetCacheability(HttpCacheability.NoCache);
            cache.SetNoStore();
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            base.OnResultExecuting(filterContext);
        }
    }
}