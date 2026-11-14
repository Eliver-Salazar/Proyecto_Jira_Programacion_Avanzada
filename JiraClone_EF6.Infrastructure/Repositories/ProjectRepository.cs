using System;

using System.Linq;

using System.Threading.Tasks;

using System.Data.Entity;

using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Infrastructure.Repositories
{
    public sealed class ProjectRepository : IProjectRepository
    {
        private readonly JiraCloneDBEntities1 _context;

        public ProjectRepository(JiraCloneDBEntities1 context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        public Task<ProjectSummary> GetByIdAsync(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(projectId),
                    "El identificador del proyecto debe ser mayor que cero.");
            }

            return _context.Projects
                .AsNoTracking()
                .Where(project => project.ProjectId == projectId)
                .Select(project => new ProjectSummary
                {
                    ProjectId = project.ProjectId,
                    ProjectKey = project.ProjectKey,
                    Name = project.Name,
                    IsActive = project.IsActive
                })
                .SingleOrDefaultAsync();
        }
    }
}
