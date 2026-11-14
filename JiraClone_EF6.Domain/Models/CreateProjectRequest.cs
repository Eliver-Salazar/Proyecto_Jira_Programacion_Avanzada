using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Datos necesarios para crear un proyecto.
    /// El propietario se recibe por separado desde la sesión.
    /// </summary>
    public sealed class CreateProjectRequest
    {
        public string ProjectKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
