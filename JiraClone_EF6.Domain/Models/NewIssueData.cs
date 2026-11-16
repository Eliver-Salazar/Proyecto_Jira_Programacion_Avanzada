using System;

namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Datos ya validados que se envían a dbo.usp_CreateIssue.
    /// El creador proviene de la sesión, nunca del formulario.
    /// </summary>
    public sealed class NewIssueData
    {
        public int ProjectId { get; set; }
        public int IssueTypeId { get; set; }
        public int IssuePriorityId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReporterUserId { get; set; }
        public string AssigneeUserId { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
