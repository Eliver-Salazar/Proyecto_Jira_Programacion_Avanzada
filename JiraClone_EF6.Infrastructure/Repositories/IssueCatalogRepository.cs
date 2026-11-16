using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Infrastructure.Repositories
{
    /// <summary>
    /// Lecturas sin tracking de catálogos, transiciones y miembros.
    /// </summary>
    public sealed class IssueCatalogRepository : IIssueCatalogRepository
    {
        private readonly JiraCloneDBEntities1 _context;

        public IssueCatalogRepository(JiraCloneDBEntities1 context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<LookupItem>> GetIssueTypesAsync()
        {
            return _context.IssueTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .Select(t => new LookupItem
                {
                    Id = t.IssueTypeId,
                    Name = t.Name
                })
                .ToListAsync();
        }

        public Task<List<LookupItem>> GetPrioritiesAsync()
        {
            return _context.IssuePriorities
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.PriorityLevel)
                .Select(p => new LookupItem
                {
                    Id = p.IssuePriorityId,
                    Name = p.Name
                })
                .ToListAsync();
        }

        public Task<List<LookupItem>> GetStatusesAsync()
        {
            return _context.WorkflowStatuses
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.WorkflowStatusId)
                .Select(s => new LookupItem
                {
                    Id = s.WorkflowStatusId,
                    Name = s.Name,
                    Category = s.StatusCategory
                })
                .ToListAsync();
        }

        public Task<List<LookupItem>> GetAvailableTransitionsAsync(
            int projectId, int fromStatusId)
        {
            // Solo muestra opciones; la validación final la hace el SP.
            return _context.WorkflowTransitions
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId
                            && t.FromStatusId == fromStatusId
                            && t.IsActive)
                .Join(_context.WorkflowStatuses.Where(s => s.IsActive),
                    t => t.ToStatusId,
                    s => s.WorkflowStatusId,
                    (t, s) => s)
                .OrderBy(s => s.WorkflowStatusId)
                .Select(s => new LookupItem
                {
                    Id = s.WorkflowStatusId,
                    Name = s.Name,
                    Category = s.StatusCategory
                })
                .ToListAsync();
        }

        public Task<List<ProjectMemberItem>> GetMembersAsync(int projectId)
        {
            // AspNetUsers no forma parte del EDMX; se consulta con
            // un parámetro tipado, sin concatenar valores.
            var parameter = new SqlParameter("@ProjectId", SqlDbType.Int)
            {
                Value = projectId
            };

            return _context.Database
                .SqlQuery<ProjectMemberItem>(
                    "SELECT pm.UserId, u.UserName "
                    + "FROM dbo.ProjectMembers AS pm "
                    + "INNER JOIN dbo.AspNetUsers AS u ON u.Id = pm.UserId "
                    + "WHERE pm.ProjectId = @ProjectId "
                    + "ORDER BY u.UserName",
                    parameter)
                .ToListAsync();
        }
    }
}
