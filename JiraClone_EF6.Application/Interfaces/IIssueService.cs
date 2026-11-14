using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JiraClone_EF6.Application.DTOs.Common;
using JiraClone_EF6.Application.DTOs.Issues;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Contrato inicial del módulo de tareas, implementado por Carlos.
    /// </summary>
    public interface IIssueService
    {
        // Requiere CanViewAsync. Devuelve una página filtrada por proyecto.
        // page comienza en 1 y pageSize debe estar entre 1 y 100.
        Task<PagedResult<IssueSummaryDto>> GetByProjectAsync(
            int projectId,
            string userId,
            int page,
            int pageSize);

        // Devuelve null si la tarea no existe, pertenece a otro proyecto
        // o el usuario no tiene permiso de lectura.
        Task<IssueSummaryDto> GetSummaryAsync(
            int projectId,
            long issueId,
            string userId);

        // Requiere CanWriteAsync y AllowIssueCreation.
        // La implementación debe invocar dbo.usp_CreateIssue.
        // Devuelve la tarea creada, incluyendo su identificador calculado.
        Task<IssueSummaryDto> CreateAsync(
            CreateIssueRequest request,
            string userId);
    }
}
