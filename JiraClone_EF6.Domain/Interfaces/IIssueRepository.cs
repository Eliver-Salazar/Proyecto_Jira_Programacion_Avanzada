using System.Collections.Generic;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Domain.Interfaces
{
    /// <summary>
    /// Contrato de persistencia del módulo de tareas.
    /// Todas las consultas filtran por proyecto.
    /// </summary>
    public interface IIssueRepository
    {
        // Tareas de un proyecto, de la más reciente a la más antigua.
        Task<List<IssueSummary>> GetByProjectAsync(
            int projectId, int skip, int take);

        // Devuelve null si la tarea no existe o es de otro proyecto.
        Task<IssueSummary> GetSummaryAsync(int projectId, long issueId);

        // Devuelve null si la tarea no existe o es de otro proyecto.
        Task<IssueDetails> GetDetailsAsync(int projectId, long issueId);

        // Lee ProjectSettings.AllowIssueCreation (false si no hay registro).
        Task<bool> IsIssueCreationAllowedAsync(int projectId);

        // Ejecuta dbo.usp_CreateIssue y devuelve el IssueId generado.
        Task<long> CreateAsync(NewIssueData data);

        // Ejecuta dbo.usp_ChangeIssueStatus.
        Task ChangeStatusAsync(
            long issueId, int newStatusId, string changedByUserId);
    }
}
