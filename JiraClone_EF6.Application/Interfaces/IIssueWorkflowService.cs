using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Punto único de cambio de estado, implementado por Carlos.
    /// Puede ser consumido por tareas y tableros.
    /// </summary>
    public interface IIssueWorkflowService
    {
        // Requiere CanWriteAsync y comprobar que la tarea pertenece al proyecto.
        // Invoca dbo.usp_ChangeIssueStatus.
        // No debe actualizar WorkflowStatusId directamente con EF6.
        Task ChangeStatusAsync(
            int projectId,
            long issueId,
            int newStatusId,
            string userId);
    }
}
