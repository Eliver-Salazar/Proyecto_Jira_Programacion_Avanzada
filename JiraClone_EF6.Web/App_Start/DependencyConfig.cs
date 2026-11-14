using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Application.Services;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Infrastructure;
using JiraClone_EF6.Infrastructure.Repositories;

namespace JiraClone_EF6.Web
{
    /// <summary>
    /// Configura los controladores y las dependencias de la aplicación.
    /// </summary>
    public static class DependencyConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Registra los controladores MVC.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Comparte el contexto funcional durante la solicitud HTTP.
            // Autofac lo libera al finalizarla.
            builder.RegisterType<JiraCloneDBEntities1>()
                .AsSelf()
                .InstancePerRequest();

            // Repositorio de consultas de proyectos.
            builder.RegisterType<ProjectRepository>()
                .As<IProjectRepository>()
                .InstancePerRequest();

            // Repositorio de comprobación de acceso.
            builder.RegisterType<ProjectAccessRepository>()
                .As<IProjectAccessRepository>()
                .InstancePerRequest();

            // Servicio compartido de autorización de lectura.
            builder.RegisterType<ProjectAccessService>()
                .As<IProjectAccessService>()
                .InstancePerRequest();

            // Servicio del módulo de proyectos.
            // Comparte las dependencias registradas para la solicitud actual.
            builder.RegisterType<ProjectService>()
                .As<IProjectService>()
                .InstancePerRequest();

            // Identity continúa administrado mediante OWIN.
            var container = builder.Build();

            DependencyResolver.SetResolver(
                new AutofacDependencyResolver(container));
        }
    }
}