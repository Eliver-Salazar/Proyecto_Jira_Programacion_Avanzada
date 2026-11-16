using System.Collections.Generic;

namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Catálogos y miembros necesarios para formularios y listados de tareas.
    /// </summary>
    public sealed class IssueFormOptionsDto
    {
        public IssueFormOptionsDto()
        {
            IssueTypes = new List<LookupItemDto>();
            Priorities = new List<LookupItemDto>();
            Statuses = new List<LookupItemDto>();
            Members = new List<ProjectMemberDto>();
        }

        public List<LookupItemDto> IssueTypes { get; set; }
        public List<LookupItemDto> Priorities { get; set; }
        public List<LookupItemDto> Statuses { get; set; }
        public List<ProjectMemberDto> Members { get; set; }

        // Indica si el usuario puede crear o modificar tareas.
        public bool CanWrite { get; set; }
    }
}
