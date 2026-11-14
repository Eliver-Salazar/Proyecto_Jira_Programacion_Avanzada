using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JiraClone_EF6.Web.ViewModels.Projects
{
    /// <summary>
    /// Datos y navegación del listado paginado.
    /// </summary>
    public sealed class ProjectIndexViewModel
    {
        public ProjectIndexViewModel()
        {
            Projects = new List<ProjectDetailsViewModel>();
        }

        public List<ProjectDetailsViewModel> Projects { get; set; }
        public int Page { get; set; }
        public bool HasNextPage { get; set; }
    }
}