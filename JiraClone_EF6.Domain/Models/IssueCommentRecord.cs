using System;
namespace JiraClone_EF6.Domain.Models
{
    // Modelo independiente de Entity Framework.
    public sealed class IssueCommentRecord
    {
        public long IssueCommentId { get; set; }
        public long IssueId { get; set; }
        public string AuthorUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
    }
}
