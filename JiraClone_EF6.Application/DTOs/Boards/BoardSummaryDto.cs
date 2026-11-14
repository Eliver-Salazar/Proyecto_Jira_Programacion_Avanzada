using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Application.DTOs.Boards
{
    /// <summary>
    /// Información básica de un tablero.
    /// </summary>
    public sealed class BoardSummaryDto
    {
        public int BoardId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }

        // Debe respetar los valores definidos por el CHECK de SQL Server.
        public string BoardType { get; set; }
    }
}
