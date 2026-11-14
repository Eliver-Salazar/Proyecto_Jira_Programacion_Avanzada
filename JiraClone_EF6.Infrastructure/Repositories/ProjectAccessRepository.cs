using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Interfaces;

namespace JiraClone_EF6.Infrastructure.Repositories
{
    /// <summary>
    /// Comprueba permisos mediante consultas a SQL Server.
    /// No carga entidades ni mantiene resultados de permisos en caché.
    /// </summary>
    public sealed class ProjectAccessRepository : IProjectAccessRepository
    {
        private readonly JiraCloneDBEntities1 _context;

        public ProjectAccessRepository(JiraCloneDBEntities1 context)
        {
            // Autofac proporciona y libera el contexto por solicitud.
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<bool> CanViewAsync(int projectId, string userId)
        {
            // Todos los roles reconocidos permiten consultar.
            return HasAccessAsync(
                projectId,
                userId,
                new[] { "ADMIN", "MEMBER", "VIEWER" });
        }

        public Task<bool> CanWriteAsync(int projectId, string userId)
        {
            // VIEWER no puede modificar contenido.
            return HasAccessAsync(
                projectId,
                userId,
                new[] { "ADMIN", "MEMBER" });
        }

        public Task<bool> CanManageAsync(int projectId, string userId)
        {
            // La administración queda reservada a ADMIN y al propietario.
            return HasAccessAsync(
                projectId,
                userId,
                new[] { "ADMIN" });
        }

        private Task<bool> HasAccessAsync(
            int projectId,
            string userId,
            string[] allowedRoles)
        {
            // Una entrada inválida siempre deniega el acceso.
            if (projectId <= 0 || string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(false);
            }

            // EF6 traduce la comprobación completa a SQL.
            // El propietario conserva acceso aunque falte su membresía.
            return _context.Projects.AnyAsync(project =>
                project.ProjectId == projectId
                && project.IsActive
                && (
                    project.OwnerUserId == userId
                    || project.ProjectMembers.Any(member =>
                        member.UserId == userId
                        && allowedRoles.Contains(member.ProjectRole))
                ));
        }
    }
}