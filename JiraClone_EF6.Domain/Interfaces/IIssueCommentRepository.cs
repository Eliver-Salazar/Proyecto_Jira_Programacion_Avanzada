using System.Collections.Generic;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Models;
namespace JiraClone_EF6.Domain.Interfaces
{
    public interface IIssueCommentRepository
    {
        Task<bool> IssueBelongsToProjectAsync(int projectId, long issueId);
        Task<List<IssueCommentRecord>> GetByIssueAsync(long issueId, int skip, int take);
        Task<long> AddAsync(long issueId, string content, string authorUserId);
    }
}
