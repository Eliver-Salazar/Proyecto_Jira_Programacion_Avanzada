using System.Collections.Generic;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Domain.Interfaces
{
    /// <summary>
    /// Lecturas de catálogos del módulo de tareas para listas desplegables.
    /// </summary>
    public interface IIssueCatalogRepository
    {
        // Tipos activos ordenados por nombre.
        Task<List<LookupItem>> GetIssueTypesAsync();

        // Prioridades activas ordenadas por nivel.
        Task<List<LookupItem>> GetPrioritiesAsync();

        // Estados activos, con su categoría.
        Task<List<LookupItem>> GetStatusesAsync();

        // Estados destino con transición activa desde el estado actual.
        Task<List<LookupItem>> GetAvailableTransitionsAsync(
            int projectId, int fromStatusId);

        // Miembros del proyecto con su nombre de usuario.
        Task<List<ProjectMemberItem>> GetMembersAsync(int projectId);
    }
}
