using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Application.DTOs.Boards
{
    /// <summary>
    /// Datos editables para crear un tablero dentro de un proyecto.
    /// </summary>
    public sealed class CreateBoardRequest
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BoardType { get; set; }
    }
}