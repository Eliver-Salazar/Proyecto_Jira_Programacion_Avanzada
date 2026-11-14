using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JiraClone_EF6.Application.DTOs.Boards;
using JiraClone_EF6.Application.DTOs.Common;

namespace JiraClone_EF6.Application.Interfaces
{
    /// <summary>
    /// Contrato inicial de tableros, implementado por Darien.
    /// </summary>
    public interface IBoardService
    {
        // Requiere lectura y devuelve únicamente tableros activos del proyecto.
        // page comienza en 1 y pageSize debe estar entre 1 y 100.
        Task<PagedResult<BoardSummaryDto>> GetByProjectAsync(
            int projectId,
            string userId,
            int page,
            int pageSize);

        // Devuelve null si no existe, está inactivo, pertenece a otro proyecto
        // o el usuario no tiene permiso.
        Task<BoardSummaryDto> GetByIdAsync(
            int projectId,
            int boardId,
            string userId);

        // Configurar tableros requiere CanManageAsync.
        // Devuelve el identificador del tablero creado.
        Task<int> CreateAsync(
            CreateBoardRequest request,
            string userId);
    }
}
