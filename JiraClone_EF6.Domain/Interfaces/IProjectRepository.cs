using System.Collections.Generic;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Domain.Interfaces
{
    /// <summary>
    /// Contrato de persistencia del módulo de proyectos.
    /// </summary>
    public interface IProjectRepository
    {
        // Devuelve null cuando el identificador no existe.
        Task<ProjectSummary> GetByIdAsync(int projectId);

        // Devuelve únicamente proyectos activos accesibles para el usuario.
        Task<List<ProjectSummary>> GetForUserAsync(
            string userId, int skip, int take);

        // Ayuda a informar duplicados antes de intentar guardar.
        Task<bool> KeyExistsAsync(string projectKey);

        // Guarda el proyecto y sus registros iniciales de forma atómica.
        Task<int> CreateAsync(CreateProjectRequest request, string ownerUserId);
    }
}