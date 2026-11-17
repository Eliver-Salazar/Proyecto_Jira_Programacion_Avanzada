using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Web.ViewModels.Comments;
namespace JiraClone_EF6.Web.Controllers
{
    [Authorize]
    public sealed class IssueCommentsController : Controller
    {
        private readonly IIssueCommentService _comments;
        private readonly IProjectAccessService _access;
        public IssueCommentsController(IIssueCommentService comments, IProjectAccessService access)
        {
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _access = access ?? throw new ArgumentNullException(nameof(access));
        }
        [HttpGet]
        public async Task<ActionResult> Index(int projectId, long issueId, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            try
            {
                var result = await _comments.GetByIssueAsync(projectId, issueId, userId, page, 10);
                var model = new IssueCommentsViewModel
                {
                    ProjectId = projectId, IssueId = issueId,
                    Page = result.Page, HasNextPage = result.HasNextPage,
                    Comments = result.Items,
                    CanWrite = await _access.CanWriteAsync(projectId, userId)
                };
                return View(model);
            }
            catch (UnauthorizedAccessException) { return new HttpStatusCodeResult(403); }
            catch (ArgumentException) { return HttpNotFound(); }
            catch (OverflowException) { return new HttpStatusCodeResult(400); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(IssueCommentsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["CommentError"] = "Escriba un comentario válido (máximo 10000 caracteres).";
                return RedirectToAction("Index", new { projectId = model.ProjectId, issueId = model.IssueId });
            }
            try
            {
                await _comments.AddAsync(model.ProjectId, model.IssueId,
                    model.Content, User.Identity.GetUserId());
                return RedirectToAction("Index", new { projectId = model.ProjectId, issueId = model.IssueId });
            }
            catch (UnauthorizedAccessException) { return new HttpStatusCodeResult(403); }
            catch (ArgumentException) { return new HttpStatusCodeResult(400); }
        }
    }
}
