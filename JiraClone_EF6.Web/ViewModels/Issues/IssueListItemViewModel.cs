namespace JiraClone_EF6.Web.ViewModels.Issues
{
    /// <summary>
    /// Fila del listado de tareas con nombres ya resueltos.
    /// </summary>
    public sealed class IssueListItemViewModel
    {
        public long IssueId { get; set; }
        public string IssueKey { get; set; }
        public string Title { get; set; }
        public string TypeName { get; set; }
        public string PriorityName { get; set; }
        public string StatusName { get; set; }
        public string AssigneeName { get; set; }
    }
}
