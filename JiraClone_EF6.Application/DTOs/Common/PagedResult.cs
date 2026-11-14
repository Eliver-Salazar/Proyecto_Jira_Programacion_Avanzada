using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JiraClone_EF6.Application.DTOs.Common
{
    /// <summary>
    /// Resultado paginado reutilizable, sin dependencias de EF6.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public PagedResult()
        {
            Items = new List<T>();
        }

        // Contiene únicamente los registros de la página solicitada.
        public List<T> Items { get; set; }

        // La numeración de páginas comienza en 1.
        public int Page { get; set; }
        public int PageSize { get; set; }

        // Se puede calcular consultando un registro adicional.
        public bool HasNextPage { get; set; }
    }
}
