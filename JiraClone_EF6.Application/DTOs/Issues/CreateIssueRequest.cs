using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Datos editables para crear una tarea mediante el sp SQL.
    /// </summary>
    public sealed class CreateIssueRequest
    {
        public int ProjectId { get; set; }
        public int IssueTypeId { get; set; }
        public int IssuePriorityId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AssigneeUserId { get; set; }

        // Se enviará como DATE al sp.
        public DateTime? DueDate { get; set; }
    }
}
