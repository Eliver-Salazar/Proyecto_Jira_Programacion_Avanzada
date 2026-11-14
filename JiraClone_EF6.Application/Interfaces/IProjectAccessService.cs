using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Contrato compartido de autorización para todos los módulos.
    /// El userId debe proceder de la identidad autenticada.
    /// </summary>
    public interface IProjectAccessService
    {
        // Permite consultar información del proyecto.
        Task<bool> CanViewAsync(int projectId, string userId);

        // Permite contribuir contenido, sujeto a las reglas de cada módulo.
        Task<bool> CanWriteAsync(int projectId, string userId);

        // Permite administrar el proyecto y su configuración.
        Task<bool> CanManageAsync(int projectId, string userId);
    }
}
