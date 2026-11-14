using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Models;
using JiraClone_EF6.Web.ViewModels.Projects;

namespace JiraClone_EF6.Web.Controllers
{
    /// <summary>
    /// Expone las pantallas del módulo para usuarios autenticados.
    /// </summary>
    [Authorize]
    public sealed class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService
                ?? throw new ArgumentNullException(nameof(projectService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            if (page < 1 || page > 100000)
            {
                return HttpNotFound();
            }

            // El usuario siempre se obtiene de Identity.
            var projects = await _projectService.GetForUserAsync(
                User.Identity.GetUserId(), page);

            var model = new ProjectIndexViewModel
            {
                Page = page,
                HasNextPage = projects.Count > 20,
                Projects = projects.Take(20)
                    .Select(project => new ProjectDetailsViewModel
                    {
                        ProjectId = project.ProjectId,
                        ProjectKey = project.ProjectKey,
                        Name = project.Name,
                        IsActive = project.IsActive
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return HttpNotFound();
            }

            var project = await _projectService.GetByIdAsync(
                id.Value, User.Identity.GetUserId());

            // Usa la misma respuesta para inexistencia y falta de acceso.
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(new ProjectDetailsViewModel
            {
                ProjectId = project.ProjectId,
                ProjectKey = project.ProjectKey,
                Name = project.Name,
                IsActive = project.IsActive
            });
        }

        [HttpGet]
        public ActionResult Create()
        {
            // Presenta únicamente los campos editables.
            return View(new ProjectCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProjectCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var projectId = await _projectService.CreateAsync(
                    new CreateProjectRequest
                    {
                        ProjectKey = model.ProjectKey,
                        Name = model.Name,
                        Description = model.Description
                    },
                    User.Identity.GetUserId());

                // Evita repetir el POST al actualizar el navegador.
                TempData["Success"] = "Proyecto creado correctamente.";
                return RedirectToAction("Details", new { id = projectId });
            }
            catch (DuplicateProjectKeyException)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectKey),
                    "La clave ya está registrada. Utilice otra.");
            }
            catch (ArgumentException exception)
            {
                // Muestra las validaciones controladas de Application.
                ModelState.AddModelError("", exception.Message);
            }
            catch (Exception exception)
            {
                // Registra el diagnóstico sin mostrar detalles técnicos al usuario.
                Trace.TraceError("Error al crear proyecto: {0}", exception);

                ModelState.AddModelError("",
                    "No se pudo crear el proyecto. Inténtelo nuevamente.");
            }

            return View(model);
        }
    }
}