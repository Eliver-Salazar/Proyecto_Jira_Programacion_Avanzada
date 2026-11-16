namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Miembro de un proyecto que puede ser responsable de una tarea.
    /// </summary>
    public sealed class ProjectMemberItem
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
