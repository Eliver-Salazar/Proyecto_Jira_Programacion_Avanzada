using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Domain.Exceptions
{
    /// <summary>
    /// Representa el conflicto de una clave de proyecto ya registrada.
    /// Evita exponer errores específicos de SQL Server a otras capas.
    /// </summary>
    public sealed class DuplicateProjectKeyException : Exception
    {
        public DuplicateProjectKeyException()
            : base("La clave del proyecto ya está registrada.")
        {
        }

        public DuplicateProjectKeyException(Exception innerException)
            : base("La clave del proyecto ya está registrada.", innerException)
        {
        }
    }
}
