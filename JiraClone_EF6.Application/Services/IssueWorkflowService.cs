using System;
using System.Threading.Tasks;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Interfaces;

namespace JiraClone_EF6.Application.Services
{
    /// <summary>
    /// Punto único para cambiar el estado de una tarea. Lo usan Tareas y
    /// Tableros. Las reglas de transición viven en dbo.usp_ChangeIssueStatus;
    /// aquí no se duplican ni se actualiza WorkflowStatusId con EF6.
    ///
    /// Errores posibles para quien lo consuma:
    /// - UnauthorizedAccessException: sin permiso de escritura.
    /// - IssueNotFoundException: la tarea no existe en ese proyecto.
    /// - IssueRuleException: mismo estado o transición no permitida.
    /// </summary>
    public sealed class IssueWorkflowService : IIssueWorkflowService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectAccessService _accessService;

        public IssueWorkflowService(
            IIssueRepository issueRepository,
            IProjectAccessService accessService)
        {
            _issueRepository = issueRepository
                ?? throw new ArgumentNullException(nameof(issueRepository));

            _accessService = accessService
                ?? throw new ArgumentNullException(nameof(accessService));
        }

        public async Task ChangeStatusAsync(
            int projectId, long issueId, int newStatusId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Debe iniciar sesión para cambiar el estado.");
            }

            if (projectId <= 0 || issueId <= 0)
            {
                throw new IssueNotFoundException();
            }

            if (newStatusId <= 0)
            {
                throw new ArgumentException("Seleccione un estado válido.");
            }

            // VIEWER y usuarios ajenos no pueden cambiar estados.
            if (!await _accessService.CanWriteAsync(projectId, userId))
            {
                throw new UnauthorizedAccessException(
                    "No tiene permiso para modificar tareas "
                    + "de este proyecto.");
            }

            // El SP no recibe ProjectId: se verifica aquí que la tarea
            // pertenezca realmente al proyecto indicado.
            var issue = await _issueRepository.GetSummaryAsync(
                projectId, issueId);

            if (issue == null)
            {
                throw new IssueNotFoundException();
            }

            await _issueRepository.ChangeStatusAsync(
                issueId, newStatusId, userId);
        }
    }
}
