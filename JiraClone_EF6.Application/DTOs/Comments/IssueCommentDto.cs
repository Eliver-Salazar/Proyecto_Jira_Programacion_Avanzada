using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace JiraClone_EF6.Application.DTOs.Comments
{
    /// <summary>
    /// Información de un comentario visible.
    /// </summary>
    public sealed class IssueCommentDto
    {
        public long IssueCommentId { get; set; }
        public long IssueId { get; set; }
        public string AuthorUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
    }
}
