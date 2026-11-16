using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace JiraClone_EF6.Web.ViewModels.Issues
{
    /// <summary>
    /// Campos editables del formulario de creación de tareas.
    /// No recibe el creador: sale de la sesión. El estado inicial solo
    /// puede ser uno de categoría TODO; el servicio lo vuelve a validar.
    /// </summary>
    public sealed class IssueCreateViewModel
    {
        public IssueCreateViewModel()
        {
            IssueTypes = new List<SelectListItem>();
            Priorities = new List<SelectListItem>();
            Statuses = new List<SelectListItem>();
            Members = new List<SelectListItem>();
        }

        // El servicio comprueba que el usuario pueda escribir en él.
        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Seleccione el tipo.")]
        [Display(Name = "Tipo")]
        public int? IssueTypeId { get; set; }

        [Required(ErrorMessage = "Seleccione la prioridad.")]
        [Display(Name = "Prioridad")]
        public int? IssuePriorityId { get; set; }

        [Required(ErrorMessage = "Seleccione el estado inicial.")]
        [Display(Name = "Estado inicial")]
        public int? WorkflowStatusId { get; set; }

        [Required(ErrorMessage = "Ingrese el título.")]
        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; }

        [StringLength(4000, ErrorMessage = "Máximo 4000 caracteres.")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Responsable")]
        public string AssigneeUserId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de vencimiento")]
        public DateTime? DueDate { get; set; }

        // Datos solo de presentación; se recargan en cada solicitud.
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<SelectListItem> IssueTypes { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}
