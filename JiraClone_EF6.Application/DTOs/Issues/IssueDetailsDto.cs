using System;
using System.Collections.Generic;

namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Detalle de una tarea con nombres de catálogos y transiciones posibles.
    /// </summary>
    public sealed class IssueDetailsDto
    {
        public IssueDetailsDto()
        {
            AvailableTransitions = new List<LookupItemDto>();
        }

        public long IssueId { get; set; }
        public int ProjectId { get; set; }
        public string IssueKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int IssueTypeId { get; set; }
        public string IssueTypeName { get; set; }

        public int IssuePriorityId { get; set; }
        public string IssuePriorityName { get; set; }

        public int WorkflowStatusId { get; set; }
        public string WorkflowStatusName { get; set; }
        public string StatusCategory { get; set; }

        public string ReporterUserId { get; set; }
        public string ReporterUserName { get; set; }
        public string AssigneeUserId { get; set; }
        public string AssigneeUserName { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Estados destino configurados en WorkflowTransitions.
        public List<LookupItemDto> AvailableTransitions { get; set; }

        // Indica si el usuario puede cambiar el estado.
        public bool CanWrite { get; set; }
    }
}
