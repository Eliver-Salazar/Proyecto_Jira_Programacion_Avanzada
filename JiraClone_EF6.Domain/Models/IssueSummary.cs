namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Datos básicos de una tarea, sin depender de Entity Framework.
    /// </summary>
    public sealed class IssueSummary
    {
        public long IssueId { get; set; }
        public int ProjectId { get; set; }

        // Clave del proyecto + "-" + consecutivo, por ejemplo APP-1.
        public string IssueKey { get; set; }

        public string Title { get; set; }
        public int IssueTypeId { get; set; }
        public int IssuePriorityId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string AssigneeUserId { get; set; }
    }
}
