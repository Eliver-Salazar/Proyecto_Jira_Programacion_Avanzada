using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using JiraClone_EF6.Application.DTOs.Issues;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Models;
using JiraClone_EF6.Web.ViewModels.Issues;

namespace JiraClone_EF6.Web.Controllers
{
    /// <summary>
    /// Pantallas del módulo de tareas, siempre dentro de un proyecto:
    /// /Issues?projectId=1, /Issues/Create?projectId=1 y
    /// /Issues/Details/5?projectId=1.
    /// </summary>
    [Authorize]
    public sealed class IssuesController : Controller
    {
        private const int PageSize = 20;

        private readonly IIssueService _issueService;
        private readonly IIssueReadService _issueReadService;
        private readonly IIssueWorkflowService _workflowService;
        private readonly IProjectService _projectService;

        public IssuesController(
            IIssueService issueService,
            IIssueReadService issueReadService,
            IIssueWorkflowService workflowService,
            IProjectService projectService)
        {
            _issueService = issueService
                ?? throw new ArgumentNullException(nameof(issueService));
            _issueReadService = issueReadService
                ?? throw new ArgumentNullException(nameof(issueReadService));
            _workflowService = workflowService
                ?? throw new ArgumentNullException(nameof(workflowService));
            _projectService = projectService
                ?? throw new ArgumentNullException(nameof(projectService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(int? projectId, int page = 1)
        {
            if (!projectId.HasValue || projectId.Value <= 0
                || page < 1 || page > 100000)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();

            // Misma respuesta para proyecto inexistente o sin acceso.
            var project = await _projectService.GetByIdAsync(
                projectId.Value, userId);

            if (project == null)
            {
                return HttpNotFound();
            }

            try
            {
                var options = await _issueReadService.GetFormOptionsAsync(
                    project.ProjectId, userId);

                var result = await _issueService.GetByProjectAsync(
                    project.ProjectId, userId, page, PageSize);

                var model = new IssueIndexViewModel
                {
                    ProjectId = project.ProjectId,
                    ProjectKey = project.ProjectKey,
                    ProjectName = project.Name,
                    Page = result.Page,
                    HasNextPage = result.HasNextPage,
                    CanWrite = options.CanWrite,
                    Issues = result.Items
                        .Select(issue => new IssueListItemViewModel
                        {
                            IssueId = issue.IssueId,
                            IssueKey = issue.IssueKey,
                            Title = issue.Title,
                            TypeName = NameOf(
                                options.IssueTypes, issue.IssueTypeId),
                            PriorityName = NameOf(
                                options.Priorities, issue.IssuePriorityId),
                            StatusName = NameOf(
                                options.Statuses, issue.WorkflowStatusId),
                            AssigneeName = MemberName(
                                options.Members, issue.AssigneeUserId)
                        })
                        .ToList()
                };

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int? projectId, long? id)
        {
            if (!projectId.HasValue || projectId.Value <= 0
                || !id.HasValue || id.Value <= 0)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();

            var project = await _projectService.GetByIdAsync(
                projectId.Value, userId);

            if (project == null)
            {
                return HttpNotFound();
            }

            // null si no existe, es de otro proyecto o no hay acceso.
            var issue = await _issueReadService.GetDetailsAsync(
                project.ProjectId, id.Value, userId);

            if (issue == null)
            {
                return HttpNotFound();
            }

            return View(ToDetailsViewModel(project, issue));
        }

        [HttpGet]
        public async Task<ActionResult> Create(int? projectId)
        {
            if (!projectId.HasValue || projectId.Value <= 0)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();

            var project = await _projectService.GetByIdAsync(
                projectId.Value, userId);

            if (project == null)
            {
                return HttpNotFound();
            }

            var model = new IssueCreateViewModel
            {
                ProjectId = project.ProjectId
            };

            var options = await LoadCreateOptionsAsync(model, project, userId);

            // VIEWER puede ver el proyecto pero no crear tareas.
            if (!options.CanWrite)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IssueCreateViewModel model)
        {
            if (model == null || model.ProjectId <= 0)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();

            var project = await _projectService.GetByIdAsync(
                model.ProjectId, userId);

            if (project == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _issueService.CreateAsync(
                        new CreateIssueRequest
                        {
                            ProjectId = model.ProjectId,
                            IssueTypeId = model.IssueTypeId.Value,
                            IssuePriorityId = model.IssuePriorityId.Value,

                            // El servicio valida que sea de categoría TODO.
                            WorkflowStatusId = model.WorkflowStatusId.Value,
                            Title = model.Title,
                            Description = model.Description,
                            AssigneeUserId = model.AssigneeUserId,
                            DueDate = model.DueDate
                        },
                        userId);

                    // Evita repetir el POST al actualizar el navegador.
                    TempData["Success"] =
                        "Tarea " + created.IssueKey + " creada correctamente.";

                    return RedirectToAction("Details", new
                    {
                        id = created.IssueId,
                        projectId = created.ProjectId
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                catch (ArgumentException exception)
                {
                    // Validaciones controladas de Application.
                    ModelState.AddModelError("", exception.Message);
                }
                catch (IssueRuleException exception)
                {
                    ModelState.AddModelError("", exception.Message);
                }
                catch (Exception exception)
                {
                    // Se registra el detalle sin mostrarlo al usuario.
                    Trace.TraceError("Error al crear tarea: {0}", exception);

                    ModelState.AddModelError("",
                        "No se pudo crear la tarea. Inténtelo nuevamente.");
                }
            }

            await LoadCreateOptionsAsync(model, project, userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStatus(
            int projectId, long id, int? newStatusId)
        {
            if (projectId <= 0 || id <= 0)
            {
                return HttpNotFound();
            }

            if (!newStatusId.HasValue)
            {
                TempData["Error"] = "Seleccione el nuevo estado.";
                return RedirectToAction("Details", new { id, projectId });
            }

            try
            {
                await _workflowService.ChangeStatusAsync(
                    projectId, id, newStatusId.Value,
                    User.Identity.GetUserId());

                TempData["Success"] = "Estado actualizado correctamente.";
            }
            catch (IssueNotFoundException)
            {
                return HttpNotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            catch (IssueRuleException exception)
            {
                TempData["Error"] = exception.Message;
            }
            catch (ArgumentException exception)
            {
                TempData["Error"] = exception.Message;
            }
            catch (Exception exception)
            {
                Trace.TraceError("Error al cambiar estado: {0}", exception);
                TempData["Error"] =
                    "No se pudo cambiar el estado. Inténtelo nuevamente.";
            }

            return RedirectToAction("Details", new { id, projectId });
        }

        /// <summary>
        /// Carga las listas del formulario y los datos del proyecto.
        /// </summary>
        private async Task<IssueFormOptionsDto> LoadCreateOptionsAsync(
            IssueCreateViewModel model, ProjectSummary project, string userId)
        {
            var options = await _issueReadService.GetFormOptionsAsync(
                project.ProjectId, userId);

            model.ProjectKey = project.ProjectKey;
            model.ProjectName = project.Name;

            model.IssueTypes = options.IssueTypes
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = model.IssueTypeId == t.Id
                })
                .ToList();

            model.Priorities = options.Priorities
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = model.IssuePriorityId == p.Id
                })
                .ToList();

            // Solo estados iniciales (TODO). Si no hay uno elegido,
            // se preselecciona el primero (Pendiente en el seed).
            var initialStatuses = options.Statuses
                .Where(s => s.Category == "TODO")
                .ToList();

            if (!model.WorkflowStatusId.HasValue && initialStatuses.Count > 0)
            {
                model.WorkflowStatusId = initialStatuses[0].Id;
            }

            model.Statuses = initialStatuses
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = model.WorkflowStatusId == s.Id
                })
                .ToList();

            model.Members = options.Members
                .Select(m => new SelectListItem
                {
                    Value = m.UserId,
                    Text = m.UserName,
                    Selected = model.AssigneeUserId == m.UserId
                })
                .ToList();

            return options;
        }

        private static IssueDetailsViewModel ToDetailsViewModel(
            ProjectSummary project, IssueDetailsDto issue)
        {
            return new IssueDetailsViewModel
            {
                ProjectId = project.ProjectId,
                ProjectKey = project.ProjectKey,
                ProjectName = project.Name,
                IssueId = issue.IssueId,
                IssueKey = issue.IssueKey,
                Title = issue.Title,
                Description = issue.Description,
                TypeName = issue.IssueTypeName,
                PriorityName = issue.IssuePriorityName,
                StatusName = issue.WorkflowStatusName,
                ReporterName = issue.ReporterUserName ?? "(no disponible)",
                AssigneeName = issue.AssigneeUserName ?? "Sin asignar",
                DueDate = issue.DueDate,
                CreatedAt = issue.CreatedAt,
                UpdatedAt = issue.UpdatedAt,
                CompletedAt = issue.CompletedAt,
                CanWrite = issue.CanWrite,
                Transitions = issue.AvailableTransitions
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToList()
            };
        }

        private static string NameOf(List<LookupItemDto> items, int id)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            return item != null ? item.Name : "-";
        }

        private static string MemberName(
            List<ProjectMemberDto> members, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return "Sin asignar";
            }

            var member = members.FirstOrDefault(m => m.UserId == userId);
            return member != null ? member.UserName : "-";
        }
    }
}
