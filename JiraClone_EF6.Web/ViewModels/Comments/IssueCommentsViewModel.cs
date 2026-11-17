using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JiraClone_EF6.Application.DTOs.Comments;
namespace JiraClone_EF6.Web.ViewModels.Comments
{
    public sealed class IssueCommentsViewModel
    {
        public int ProjectId { get; set; }
        public long IssueId { get; set; }
        public int Page { get; set; }
        public bool HasNextPage { get; set; }
        public bool CanWrite { get; set; }
        public List<IssueCommentDto> Comments { get; set; } = new List<IssueCommentDto>();
        [Required(ErrorMessage = "Escriba un comentario.")]
        [StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
        [Display(Name = "Comentario")]
        public string Content { get; set; }
    }
}
