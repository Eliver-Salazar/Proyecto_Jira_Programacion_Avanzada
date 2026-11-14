using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Domain.Interfaces
{
    //Definir qué consulta estará disponible. Application podrá utilizarlo sin referenciar Infrastructure.
    public interface IProjectRepository
    {
        // Devuelve null si el proyecto no existe.
        Task<ProjectSummary> GetByIdAsync(int projectId);
    }
}
