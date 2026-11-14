using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JiraClone_EF6.Application.DTOs.Comments;
using JiraClone_EF6.Application.DTOs.Common;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Contrato inicial de colaboración, implementado por Raquel.
    /// </summary>
    public interface IIssueCommentService
    {
        // Requiere lectura y verificar la pertenencia de la tarea al proyecto.
        // Excluye comentarios con IsDeleted = true.
        // page comienza en 1 y pageSize debe estar entre 1 y 100.
        Task<PagedResult<IssueCommentDto>> GetByIssueAsync(
            int projectId,
            long issueId,
            string userId,
            int page,
            int pageSize);

        // Requiere CanWriteAsync y comprobar la pertenencia de la tarea.
        // El autor se obtiene de userId, nunca de un campo del formulario.
        Task<long> AddAsync(
            int projectId,
            long issueId,
            string content,
            string userId);
    }
}
