using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace JiraClone_EF6.Web.ViewModels.Projects
{
    /// <summary>
    /// Campos editables del formulario de creación.
    /// No permite recibir propietario, roles ni estado desde el navegador.
    /// </summary>
    public sealed class ProjectCreateViewModel
    {
        [Required(ErrorMessage = "Ingrese la clave.")]
        [StringLength(20, MinimumLength = 2,
            ErrorMessage = "La clave debe tener entre 2 y 20 caracteres.")]
        [RegularExpression(@"[A-Za-z][A-Za-z0-9]{1,19}",
            ErrorMessage = "Use letras y números, empezando con una letra.")]
        [Display(Name = "Clave")]
        public string ProjectKey { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }
    }
}