using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Infrastructure.Repositories
{
    /// <summary>
    /// Implementa consultas y creación de proyectos mediante EF6.
    /// El contexto pertenece a la solicitud y lo libera Autofac.
    /// </summary>
    public sealed class ProjectRepository : IProjectRepository
    {
        private readonly JiraCloneDBEntities1 _context;

        public ProjectRepository(JiraCloneDBEntities1 context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<ProjectSummary> GetByIdAsync(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            // Proyecta únicamente los datos que necesita Application.
            return _context.Projects
                .AsNoTracking()
                .Where(project => project.ProjectId == projectId)
                .Select(project => new ProjectSummary
                {
                    ProjectId = project.ProjectId,
                    ProjectKey = project.ProjectKey,
                    Name = project.Name,
                    IsActive = project.IsActive
                })
                .SingleOrDefaultAsync();
        }

        public Task<List<ProjectSummary>> GetForUserAsync(
            string userId, int skip, int take)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("El usuario es obligatorio.");
            }

            if (skip < 0 || take <= 0 || take > 101)
            {
                throw new ArgumentOutOfRangeException(nameof(take));
            }

            // El filtro de acceso se aplica en SQL antes de paginar.
            return _context.Projects
                .AsNoTracking()
                .Where(project =>
                    project.IsActive
                    && (
                        project.OwnerUserId == userId
                        || project.ProjectMembers.Any(member =>
                            member.UserId == userId
                            && (
                                member.ProjectRole == "ADMIN"
                                || member.ProjectRole == "MEMBER"
                                || member.ProjectRole == "VIEWER"
                            ))
                    ))
                .OrderByDescending(project => project.CreatedAt)
                .ThenByDescending(project => project.ProjectId)
                .Skip(skip)
                .Take(take)
                .Select(project => new ProjectSummary
                {
                    ProjectId = project.ProjectId,
                    ProjectKey = project.ProjectKey,
                    Name = project.Name,
                    IsActive = project.IsActive
                })
                .ToListAsync();
        }

        public Task<bool> KeyExistsAsync(string projectKey)
        {
            // Incluye proyectos inactivos porque la clave es única globalmente.
            return _context.Projects.AnyAsync(
                project => project.ProjectKey == projectKey);
        }

        public async Task<int> CreateAsync(
            CreateProjectRequest request,
            string ownerUserId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                throw new ArgumentException("El propietario es obligatorio.");
            }

            var now = DateTime.UtcNow;

            var project = new Projects
            {
                ProjectKey = request.ProjectKey,
                Name = request.Name,
                Description = request.Description,
                OwnerUserId = ownerUserId,
                IsActive = true,
                CreatedAt = now
            };

            // EF6 asignará las FK después de obtener el ProjectId generado.
            project.ProjectMembers.Add(new ProjectMembers
            {
                UserId = ownerUserId,
                ProjectRole = "ADMIN",
                JoinedAt = now
            });

            project.ProjectSettings = new ProjectSettings
            {
                AllowIssueCreation = true,
                AllowAttachments = false,
                EnableSprints = true,
                UpdatedAt = now
            };

            // Los adjuntos quedan deshabilitados hasta definir almacenamiento.
            project.ProjectIssueCounters = new ProjectIssueCounters
            {
                LastIssueNumber = 0,
                UpdatedAt = now
            };

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Guarda el grafo completo dentro de la misma transacción.
                    _context.Projects.Add(project);
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                    return project.ProjectId;
                }
                catch (DbUpdateException exception)
                {
                    transaction.Rollback();

                    // La restricción SQL resuelve también duplicados concurrentes.
                    if (IsDuplicateProjectKey(exception))
                    {
                        throw new DuplicateProjectKeyException(exception);
                    }

                    // Otros errores conservan su información para diagnóstico.
                    throw;
                }
            }
        }

        private static bool IsDuplicateProjectKey(Exception exception)
        {
            // Solo transforma el conflicto de esta restricción concreta.
            for (var current = exception;
                 current != null;
                 current = current.InnerException)
            {
                var sqlException = current as SqlException;

                if (sqlException == null)
                {
                    continue;
                }

                foreach (SqlError error in sqlException.Errors)
                {
                    if ((error.Number == 2601 || error.Number == 2627)
                        && error.Message.IndexOf(
                            "UQ_Projects_ProjectKey",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}