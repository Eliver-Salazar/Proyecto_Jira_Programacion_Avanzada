using System.Collections.Generic;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Casos de uso disponibles para el módulo de proyectos.
    /// </summary>
    public interface IProjectService
    {
        Task<ProjectSummary> GetByIdAsync(int projectId, string userId);

        // Recupera una página y un registro adicional para detectar la siguiente.
        Task<List<ProjectSummary>> GetForUserAsync(string userId, int page);

        // El usuario autenticado será el propietario del proyecto.
        Task<int> CreateAsync(CreateProjectRequest request, string userId);
    }
}