using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Información compartida de una tarea para listados y tarjetas.
    /// </summary>
    public sealed class IssueSummaryDto
    {
        public long IssueId { get; set; }
        public int ProjectId { get; set; }

        // Valor calculado, por ejemplo APP-1; no es una columna nueva.
        public string IssueKey { get; set; }

        public string Title { get; set; }
        public int IssueTypeId { get; set; }
        public int IssuePriorityId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string AssigneeUserId { get; set; }
    }
}
