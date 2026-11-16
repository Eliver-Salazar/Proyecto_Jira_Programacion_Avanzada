using System;

namespace JiraClone_EF6.Domain.Exceptions
{
    /// <summary>
    /// Regla de negocio de tareas que no se cumplió (por ejemplo, una
    /// transición no permitida). El mensaje se puede mostrar al usuario.
    /// </summary>
    public class IssueRuleException : Exception
    {
        public IssueRuleException(string message)
            : base(message)
        {
        }

        public IssueRuleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
