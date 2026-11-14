using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Domain.Interfaces
{
    /// <summary>
    /// Define las consultas de permisos internos de un proyecto.
    /// No depende de Entity Framework ni de ASP.NET Identity.
    /// </summary>
    public interface IProjectAccessRepository
    {
        // Propietario, ADMIN, MEMBER o VIEWER de un proyecto activo.
        Task<bool> CanViewAsync(int projectId, string userId);

        // Propietario, ADMIN o MEMBER de un proyecto activo.
        Task<bool> CanWriteAsync(int projectId, string userId);

        // Propietario o ADMIN de un proyecto activo.
        Task<bool> CanManageAsync(int projectId, string userId);
    }
}
