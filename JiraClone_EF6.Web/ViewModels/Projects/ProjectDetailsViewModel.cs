using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JiraClone_EF6.Web.ViewModels.Projects
{
    /// <summary>
    /// Contiene únicamente los datos que necesita la vista de detalle.
    /// </summary>
    public sealed class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}