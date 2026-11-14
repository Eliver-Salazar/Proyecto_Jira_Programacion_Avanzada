using System;
using System.Threading.Tasks;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Interfaces;

namespace JiraClone_EF6.Application.Services
{
    /// <summary>
    /// Ofrece una entrada común para comprobar permisos.
    /// Los servicios de módulos dependen de esta interfaz.
    /// </summary>
    public sealed class ProjectAccessService : IProjectAccessService
    {
        private readonly IProjectAccessRepository _accessRepository;

        public ProjectAccessService(
            IProjectAccessRepository accessRepository)
        {
            _accessRepository = accessRepository
                ?? throw new ArgumentNullException(nameof(accessRepository));
        }

        public Task<bool> CanViewAsync(int projectId, string userId)
        {
            if (!HasValidIdentity(projectId, userId))
            {
                return Task.FromResult(false);
            }

            return _accessRepository.CanViewAsync(projectId, userId);
        }

        public Task<bool> CanWriteAsync(int projectId, string userId)
        {
            if (!HasValidIdentity(projectId, userId))
            {
                return Task.FromResult(false);
            }

            return _accessRepository.CanWriteAsync(projectId, userId);
        }

        public Task<bool> CanManageAsync(int projectId, string userId)
        {
            if (!HasValidIdentity(projectId, userId))
            {
                return Task.FromResult(false);
            }

            return _accessRepository.CanManageAsync(projectId, userId);
        }

        private static bool HasValidIdentity(int projectId, string userId)
        {
            // No normaliza el identificador: debe coincidir con Identity.
            return projectId > 0 && !string.IsNullOrWhiteSpace(userId);
        }
    }
}