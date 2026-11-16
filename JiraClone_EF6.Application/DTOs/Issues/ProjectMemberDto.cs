namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Miembro del proyecto que puede ser responsable de una tarea.
    /// </summary>
    public sealed class ProjectMemberDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
