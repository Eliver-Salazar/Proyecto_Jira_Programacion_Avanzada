using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace JiraClone_EF6.Web.ViewModels.Issues
{
    /// <summary>
    /// Datos de la pantalla de detalle y del formulario de cambio de estado.
    /// </summary>
    public sealed class IssueDetailsViewModel
    {
        public IssueDetailsViewModel()
        {
            Transitions = new List<SelectListItem>();
        }

        public int ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }

        public long IssueId { get; set; }
        public string IssueKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TypeName { get; set; }
        public string PriorityName { get; set; }
        public string StatusName { get; set; }
        public string ReporterName { get; set; }
        public string AssigneeName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public bool CanWrite { get; set; }

        // Estados a los que se puede mover la tarea desde el actual.
        public IEnumerable<SelectListItem> Transitions { get; set; }
    }
}
