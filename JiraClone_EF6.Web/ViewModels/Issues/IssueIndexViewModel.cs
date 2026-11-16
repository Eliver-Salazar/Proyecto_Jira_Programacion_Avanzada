using System.Collections.Generic;

namespace JiraClone_EF6.Web.ViewModels.Issues
{
    /// <summary>
    /// Listado paginado de tareas de un proyecto.
    /// </summary>
    public sealed class IssueIndexViewModel
    {
        public IssueIndexViewModel()
        {
            Issues = new List<IssueListItemViewModel>();
        }

        public int ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }

        public List<IssueListItemViewModel> Issues { get; set; }
        public int Page { get; set; }
        public bool HasNextPage { get; set; }

        // Solo controla si se muestra el botón; el servicio valida igual.
        public bool CanWrite { get; set; }
    }
}
