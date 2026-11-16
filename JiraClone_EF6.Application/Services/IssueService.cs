using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Application.DTOs.Common;
using JiraClone_EF6.Application.DTOs.Issues;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Application.Services
{
    /// <summary>
    /// Reglas del módulo de tareas: permisos, validaciones y mapeo a DTO.
    /// Las llamadas a repositorios son secuenciales porque comparten el
    /// mismo contexto EF6 de la solicitud.
    /// </summary>
    public sealed class IssueService : IIssueService, IIssueReadService
    {
        private const int MaxTitleLength = 250;
        private const int MaxDescriptionLength = 4000;

        private readonly IIssueRepository _issueRepository;
        private readonly IIssueCatalogRepository _catalogRepository;
        private readonly IProjectAccessService _accessService;

        public IssueService(
            IIssueRepository issueRepository,
            IIssueCatalogRepository catalogRepository,
            IProjectAccessService accessService)
        {
            _issueRepository = issueRepository
                ?? throw new ArgumentNullException(nameof(issueRepository));

            _catalogRepository = catalogRepository
                ?? throw new ArgumentNullException(nameof(catalogRepository));

            _accessService = accessService
                ?? throw new ArgumentNullException(nameof(accessService));
        }

        public async Task<PagedResult<IssueSummaryDto>> GetByProjectAsync(
            int projectId, string userId, int page, int pageSize)
        {
            ValidateUser(userId);

            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (page < 1 || page > 100000)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            // Primero el permiso; solo después se consulta la base.
            if (!await _accessService.CanViewAsync(projectId, userId))
            {
                throw new UnauthorizedAccessException(
                    "No tiene acceso a este proyecto.");
            }

            // Se pide un registro extra para saber si hay otra página.
            var items = await _issueRepository.GetByProjectAsync(
                projectId, (page - 1) * pageSize, pageSize + 1);

            return new PagedResult<IssueSummaryDto>
            {
                Page = page,
                PageSize = pageSize,
                HasNextPage = items.Count > pageSize,
                Items = items.Take(pageSize).Select(ToDto).ToList()
            };
        }

        public async Task<IssueSummaryDto> GetSummaryAsync(
            int projectId, long issueId, string userId)
        {
            // Según el contrato, cualquier caso inválido devuelve null.
            if (projectId <= 0 || issueId <= 0
                || string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            if (!await _accessService.CanViewAsync(projectId, userId))
            {
                return null;
            }

            var issue = await _issueRepository.GetSummaryAsync(
                projectId, issueId);

            return issue == null ? null : ToDto(issue);
        }

        public async Task<IssueSummaryDto> CreateAsync(
            CreateIssueRequest request, string userId)
        {
            ValidateUser(userId);

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.ProjectId <= 0)
            {
                throw new ArgumentException("El proyecto no es válido.");
            }

            // VIEWER y usuarios ajenos no pueden crear tareas.
            if (!await _accessService.CanWriteAsync(request.ProjectId, userId))
            {
                throw new UnauthorizedAccessException(
                    "No tiene permiso para crear tareas en este proyecto.");
            }

            if (!await _issueRepository.IsIssueCreationAllowedAsync(
                    request.ProjectId))
            {
                throw new IssueRuleException(
                    "La creación de tareas está deshabilitada "
                    + "en este proyecto.");
            }

            var data = new NewIssueData
            {
                ProjectId = request.ProjectId,
                ReporterUserId = userId,
                Title = (request.Title ?? "").Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
                AssigneeUserId = string.IsNullOrWhiteSpace(
                        request.AssigneeUserId)
                    ? null
                    : request.AssigneeUserId.Trim(),
                DueDate = request.DueDate.HasValue
                    ? request.DueDate.Value.Date
                    : (DateTime?)null
            };

            if (data.Title.Length == 0 || data.Title.Length > MaxTitleLength)
            {
                throw new ArgumentException(
                    "El título es obligatorio y admite hasta 250 caracteres.");
            }

            if (data.Description != null
                && data.Description.Length > MaxDescriptionLength)
            {
                throw new ArgumentException(
                    "La descripción admite hasta 4000 caracteres.");
            }

            if (data.DueDate.HasValue
                && (data.DueDate.Value.Year < 2000
                    || data.DueDate.Value.Year > 2100))
            {
                throw new ArgumentException(
                    "La fecha de vencimiento no es válida.");
            }

            // Los catálogos deben existir y estar activos (sin IDs mágicos).
            var types = await _catalogRepository.GetIssueTypesAsync();
            if (!types.Any(t => t.Id == request.IssueTypeId))
            {
                throw new ArgumentException("Seleccione un tipo válido.");
            }
            data.IssueTypeId = request.IssueTypeId;

            var priorities = await _catalogRepository.GetPrioritiesAsync();
            if (!priorities.Any(p => p.Id == request.IssuePriorityId))
            {
                throw new ArgumentException(
                    "Seleccione una prioridad válida.");
            }
            data.IssuePriorityId = request.IssuePriorityId;

            data.WorkflowStatusId = await ResolveInitialStatusAsync(
                request.WorkflowStatusId);

            // El SQL también lo comprueba; aquí se da un mensaje claro.
            if (data.AssigneeUserId != null)
            {
                var members = await _catalogRepository.GetMembersAsync(
                    request.ProjectId);

                if (!members.Any(m => m.UserId == data.AssigneeUserId))
                {
                    throw new ArgumentException(
                        "El responsable debe ser miembro del proyecto.");
                }
            }

            // usp_CreateIssue asigna el consecutivo dentro de su transacción.
            var issueId = await _issueRepository.CreateAsync(data);

            // El SP solo devuelve IssueId, IssueKey y Title; se lee completa.
            var created = await _issueRepository.GetSummaryAsync(
                request.ProjectId, issueId);

            if (created == null)
            {
                throw new InvalidOperationException(
                    "No se pudo leer la tarea recién creada.");
            }

            return ToDto(created);
        }

        public async Task<IssueDetailsDto> GetDetailsAsync(
            int projectId, long issueId, string userId)
        {
            if (projectId <= 0 || issueId <= 0
                || string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            if (!await _accessService.CanViewAsync(projectId, userId))
            {
                return null;
            }

            var issue = await _issueRepository.GetDetailsAsync(
                projectId, issueId);

            if (issue == null)
            {
                return null;
            }

            var canWrite = await _accessService.CanWriteAsync(
                projectId, userId);

            var members = await _catalogRepository.GetMembersAsync(projectId);

            var transitions = canWrite
                ? await _catalogRepository.GetAvailableTransitionsAsync(
                    projectId, issue.WorkflowStatusId)
                : new List<LookupItem>();

            return new IssueDetailsDto
            {
                IssueId = issue.IssueId,
                ProjectId = issue.ProjectId,
                IssueKey = issue.IssueKey,
                Title = issue.Title,
                Description = issue.Description,
                IssueTypeId = issue.IssueTypeId,
                IssueTypeName = issue.IssueTypeName,
                IssuePriorityId = issue.IssuePriorityId,
                IssuePriorityName = issue.IssuePriorityName,
                WorkflowStatusId = issue.WorkflowStatusId,
                WorkflowStatusName = issue.WorkflowStatusName,
                StatusCategory = issue.StatusCategory,
                ReporterUserId = issue.ReporterUserId,
                ReporterUserName = FindUserName(members, issue.ReporterUserId),
                AssigneeUserId = issue.AssigneeUserId,
                AssigneeUserName = FindUserName(members, issue.AssigneeUserId),
                DueDate = issue.DueDate,
                CreatedAt = issue.CreatedAt,
                UpdatedAt = issue.UpdatedAt,
                CompletedAt = issue.CompletedAt,
                AvailableTransitions = transitions.Select(ToDto).ToList(),
                CanWrite = canWrite
            };
        }

        public async Task<IssueFormOptionsDto> GetFormOptionsAsync(
            int projectId, string userId)
        {
            ValidateUser(userId);

            if (!await _accessService.CanViewAsync(projectId, userId))
            {
                throw new UnauthorizedAccessException(
                    "No tiene acceso a este proyecto.");
            }

            var canWrite = await _accessService.CanWriteAsync(
                projectId, userId);
            var types = await _catalogRepository.GetIssueTypesAsync();
            var priorities = await _catalogRepository.GetPrioritiesAsync();
            var statuses = await _catalogRepository.GetStatusesAsync();
            var members = await _catalogRepository.GetMembersAsync(projectId);

            return new IssueFormOptionsDto
            {
                CanWrite = canWrite,
                IssueTypes = types.Select(ToDto).ToList(),
                Priorities = priorities.Select(ToDto).ToList(),
                Statuses = statuses.Select(ToDto).ToList(),
                Members = members
                    .Select(m => new ProjectMemberDto
                    {
                        UserId = m.UserId,
                        UserName = m.UserName
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Una tarea nueva siempre inicia en un estado de categoría TODO.
        /// Si no se indica, se usa el primer estado TODO activo.
        /// </summary>
        private async Task<int> ResolveInitialStatusAsync(int requestedId)
        {
            var initialStatuses = (await _catalogRepository.GetStatusesAsync())
                .Where(s => s.Category == "TODO")
                .ToList();

            if (initialStatuses.Count == 0)
            {
                throw new IssueRuleException(
                    "No hay un estado inicial configurado para las tareas.");
            }

            if (requestedId <= 0)
            {
                return initialStatuses[0].Id;
            }

            if (!initialStatuses.Any(s => s.Id == requestedId))
            {
                throw new ArgumentException(
                    "El estado inicial debe ser un estado pendiente.");
            }

            return requestedId;
        }

        private static string FindUserName(
            List<ProjectMemberItem> members, string userId)
        {
            if (userId == null)
            {
                return null;
            }

            var member = members.FirstOrDefault(m => m.UserId == userId);
            return member != null ? member.UserName : null;
        }

        private static IssueSummaryDto ToDto(IssueSummary issue)
        {
            return new IssueSummaryDto
            {
                IssueId = issue.IssueId,
                ProjectId = issue.ProjectId,
                IssueKey = issue.IssueKey,
                Title = issue.Title,
                IssueTypeId = issue.IssueTypeId,
                IssuePriorityId = issue.IssuePriorityId,
                WorkflowStatusId = issue.WorkflowStatusId,
                AssigneeUserId = issue.AssigneeUserId
            };
        }

        private static LookupItemDto ToDto(LookupItem item)
        {
            return new LookupItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Category = item.Category
            };
        }

        private static void ValidateUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Debe iniciar sesión para acceder a las tareas.");
            }
        }
    }
}
