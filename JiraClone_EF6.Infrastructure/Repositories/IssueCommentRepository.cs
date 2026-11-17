using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;
namespace JiraClone_EF6.Infrastructure.Repositories
{
    public sealed class IssueCommentRepository : IIssueCommentRepository
    {
        private readonly JiraCloneDBEntities1 _context;
        public IssueCommentRepository(JiraCloneDBEntities1 context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<bool> IssueBelongsToProjectAsync(int projectId, long issueId)
        {
            return _context.Issues.AsNoTracking().AnyAsync(i =>
                i.IssueId == issueId && i.ProjectId == projectId);
        }
        public Task<List<IssueCommentRecord>> GetByIssueAsync(long issueId, int skip, int take)
        {
            return _context.IssueComments.AsNoTracking()
                .Where(c => c.IssueId == issueId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt).ThenBy(c => c.IssueCommentId)
                .Skip(skip).Take(take)
                .Select(c => new IssueCommentRecord
                {
                    IssueCommentId = c.IssueCommentId,
                    IssueId = c.IssueId,
                    AuthorUserId = c.AuthorUserId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    IsEdited = c.IsEdited
                }).ToListAsync();
        }
        public async Task<long> AddAsync(long issueId, string content, string authorUserId)
        {
            var comment = new IssueComments
            {
                IssueId = issueId,
                AuthorUserId = authorUserId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false,
                IsDeleted = false
            };
            _context.IssueComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment.IssueCommentId;
        }
    }
}
