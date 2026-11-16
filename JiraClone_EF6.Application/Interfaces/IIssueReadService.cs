using System.Threading.Tasks;
using JiraClone_EF6.Application.DTOs.Issues;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Consultas adicionales del módulo de tareas (Carlos) para pantallas.
    /// Amplía IIssueService sin modificar ese contrato compartido.
    /// </summary>
    public interface IIssueReadService
    {
        // Devuelve null si la tarea no existe, es de otro proyecto
        // o el usuario no tiene permiso de lectura.
        Task<IssueDetailsDto> GetDetailsAsync(
            int projectId, long issueId, string userId);

        // Requiere CanViewAsync. Catálogos activos y miembros del proyecto.
        Task<IssueFormOptionsDto> GetFormOptionsAsync(
            int projectId, string userId);
    }
}
