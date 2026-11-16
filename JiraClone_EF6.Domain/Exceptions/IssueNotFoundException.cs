using System;

namespace JiraClone_EF6.Domain.Exceptions
{
    /// <summary>
    /// La tarea no existe o no pertenece al proyecto indicado.
    /// Se usa el mismo mensaje en ambos casos para no revelar datos.
    /// </summary>
    public sealed class IssueNotFoundException : IssueRuleException
    {
        public IssueNotFoundException()
            : base("La tarea no existe en este proyecto.")
        {
        }

        public IssueNotFoundException(Exception innerException)
            : base("La tarea no existe en este proyecto.", innerException)
        {
        }
    }
}
