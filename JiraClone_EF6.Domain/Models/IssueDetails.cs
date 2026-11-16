using System;

namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Información completa de una tarea para la pantalla de detalle.
    /// </summary>
    public sealed class IssueDetails
    {
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
        public string AssigneeUserId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
