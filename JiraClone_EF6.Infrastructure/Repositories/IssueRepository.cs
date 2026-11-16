using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Infrastructure.Repositories
{
    /// <summary>
    /// Consulta tareas con EF6 e invoca los procedimientos de creación y
    /// cambio de estado. El contexto pertenece a la solicitud (Autofac).
    /// </summary>
    public sealed class IssueRepository : IIssueRepository
    {
        private readonly JiraCloneDBEntities1 _context;

        public IssueRepository(JiraCloneDBEntities1 context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<IssueSummary>> GetByProjectAsync(
            int projectId, int skip, int take)
        {
            if (skip < 0 || take <= 0 || take > 101)
            {
                throw new ArgumentOutOfRangeException(nameof(take));
            }

            // Se filtra siempre por proyecto. OrderBy es obligatorio con Skip.
            var rows = await _context.Issues
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId)
                .OrderByDescending(i => i.IssueNumber)
                .Skip(skip)
                .Take(take)
                .Select(i => new
                {
                    i.IssueId,
                    i.ProjectId,
                    i.Projects.ProjectKey,
                    i.IssueNumber,
                    i.Title,
                    i.IssueTypeId,
                    i.IssuePriorityId,
                    i.WorkflowStatusId,
                    i.AssigneeUserId
                })
                .ToListAsync();

            // La clave (APP-1) se arma en memoria; no existe como columna.
            return rows.Select(r => new IssueSummary
            {
                IssueId = r.IssueId,
                ProjectId = r.ProjectId,
                IssueKey = BuildKey(r.ProjectKey, r.IssueNumber),
                Title = r.Title,
                IssueTypeId = r.IssueTypeId,
                IssuePriorityId = r.IssuePriorityId,
                WorkflowStatusId = r.WorkflowStatusId,
                AssigneeUserId = r.AssigneeUserId
            }).ToList();
        }

        public async Task<IssueSummary> GetSummaryAsync(
            int projectId, long issueId)
        {
            // Filtrar por proyecto e id evita leer tareas de otro proyecto.
            var row = await _context.Issues
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId && i.IssueId == issueId)
                .Select(i => new
                {
                    i.IssueId,
                    i.ProjectId,
                    i.Projects.ProjectKey,
                    i.IssueNumber,
                    i.Title,
                    i.IssueTypeId,
                    i.IssuePriorityId,
                    i.WorkflowStatusId,
                    i.AssigneeUserId
                })
                .SingleOrDefaultAsync();

            if (row == null)
            {
                return null;
            }

            return new IssueSummary
            {
                IssueId = row.IssueId,
                ProjectId = row.ProjectId,
                IssueKey = BuildKey(row.ProjectKey, row.IssueNumber),
                Title = row.Title,
                IssueTypeId = row.IssueTypeId,
                IssuePriorityId = row.IssuePriorityId,
                WorkflowStatusId = row.WorkflowStatusId,
                AssigneeUserId = row.AssigneeUserId
            };
        }

        public async Task<IssueDetails> GetDetailsAsync(
            int projectId, long issueId)
        {
            var row = await _context.Issues
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId && i.IssueId == issueId)
                .Select(i => new
                {
                    i.IssueId,
                    i.ProjectId,
                    i.Projects.ProjectKey,
                    i.IssueNumber,
                    i.Title,
                    i.Description,
                    i.IssueTypeId,
                    IssueTypeName = i.IssueTypes.Name,
                    i.IssuePriorityId,
                    IssuePriorityName = i.IssuePriorities.Name,
                    i.WorkflowStatusId,
                    WorkflowStatusName = i.WorkflowStatuses.Name,
                    i.WorkflowStatuses.StatusCategory,
                    i.ReporterUserId,
                    i.AssigneeUserId,
                    i.DueDate,
                    i.CreatedAt,
                    i.UpdatedAt,
                    i.CompletedAt
                })
                .SingleOrDefaultAsync();

            if (row == null)
            {
                return null;
            }

            return new IssueDetails
            {
                IssueId = row.IssueId,
                ProjectId = row.ProjectId,
                IssueKey = BuildKey(row.ProjectKey, row.IssueNumber),
                Title = row.Title,
                Description = row.Description,
                IssueTypeId = row.IssueTypeId,
                IssueTypeName = row.IssueTypeName,
                IssuePriorityId = row.IssuePriorityId,
                IssuePriorityName = row.IssuePriorityName,
                WorkflowStatusId = row.WorkflowStatusId,
                WorkflowStatusName = row.WorkflowStatusName,
                StatusCategory = row.StatusCategory,
                ReporterUserId = row.ReporterUserId,
                AssigneeUserId = row.AssigneeUserId,
                DueDate = row.DueDate,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt,
                CompletedAt = row.CompletedAt
            };
        }

        public Task<bool> IsIssueCreationAllowedAsync(int projectId)
        {
            // Sin registro de configuración se considera deshabilitado.
            return _context.ProjectSettings
                .AnyAsync(s => s.ProjectId == projectId
                               && s.AllowIssueCreation);
        }

        public async Task<long> CreateAsync(NewIssueData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Parámetros tipados con los mismos tamaños del procedimiento.
            var parameters = new[]
            {
                new SqlParameter("@ProjectId", SqlDbType.Int)
                    { Value = data.ProjectId },
                new SqlParameter("@IssueTypeId", SqlDbType.Int)
                    { Value = data.IssueTypeId },
                new SqlParameter("@IssuePriorityId", SqlDbType.Int)
                    { Value = data.IssuePriorityId },
                new SqlParameter("@WorkflowStatusId", SqlDbType.Int)
                    { Value = data.WorkflowStatusId },
                new SqlParameter("@Title", SqlDbType.NVarChar, 250)
                    { Value = data.Title },
                new SqlParameter("@Description", SqlDbType.NVarChar, -1)
                    { Value = (object)data.Description ?? DBNull.Value },
                new SqlParameter("@ReporterUserId", SqlDbType.NVarChar, 128)
                    { Value = data.ReporterUserId },
                new SqlParameter("@AssigneeUserId", SqlDbType.NVarChar, 128)
                    { Value = (object)data.AssigneeUserId ?? DBNull.Value },
                new SqlParameter("@DueDate", SqlDbType.Date)
                {
                    Value = data.DueDate.HasValue
                        ? (object)data.DueDate.Value.Date
                        : DBNull.Value
                }
            };

            try
            {
                // El procedimiento devuelve un conjunto con
                // IssueId, IssueKey y Title de la tarea creada.
                var rows = await _context.Database
                    .SqlQuery<CreatedIssueRow>(
                        "EXEC dbo.usp_CreateIssue "
                        + "@ProjectId = @ProjectId, "
                        + "@IssueTypeId = @IssueTypeId, "
                        + "@IssuePriorityId = @IssuePriorityId, "
                        + "@WorkflowStatusId = @WorkflowStatusId, "
                        + "@Title = @Title, "
                        + "@Description = @Description, "
                        + "@ReporterUserId = @ReporterUserId, "
                        + "@AssigneeUserId = @AssigneeUserId, "
                        + "@DueDate = @DueDate",
                        parameters)
                    .ToListAsync();

                if (rows.Count != 1)
                {
                    throw new InvalidOperationException(
                        "usp_CreateIssue no devolvió la tarea creada.");
                }

                return rows[0].IssueId;
            }
            catch (Exception exception)
            {
                var translated = TranslateSqlError(exception);

                if (translated != null)
                {
                    throw translated;
                }

                throw;
            }
        }

        public async Task ChangeStatusAsync(
            long issueId, int newStatusId, string changedByUserId)
        {
            var parameters = new[]
            {
                new SqlParameter("@IssueId", SqlDbType.BigInt)
                    { Value = issueId },
                new SqlParameter("@NewStatusId", SqlDbType.Int)
                    { Value = newStatusId },
                new SqlParameter("@ChangedByUserId", SqlDbType.NVarChar, 128)
                    { Value = changedByUserId }
            };

            try
            {
                // Se consume el conjunto de resultados para que el
                // procedimiento se ejecute por completo.
                await _context.Database
                    .SqlQuery<StatusChangedRow>(
                        "EXEC dbo.usp_ChangeIssueStatus "
                        + "@IssueId = @IssueId, "
                        + "@NewStatusId = @NewStatusId, "
                        + "@ChangedByUserId = @ChangedByUserId",
                        parameters)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                var translated = TranslateSqlError(exception);

                if (translated != null)
                {
                    throw translated;
                }

                throw;
            }
        }

        private static string BuildKey(string projectKey, int issueNumber)
        {
            return projectKey + "-" + issueNumber;
        }

        /// <summary>
        /// Convierte los THROW de los procedimientos en excepciones de
        /// dominio con mensajes para el usuario. Otros errores se conservan.
        /// </summary>
        private static Exception TranslateSqlError(Exception exception)
        {
            for (var current = exception;
                 current != null;
                 current = current.InnerException)
            {
                var sqlException = current as SqlException;

                if (sqlException == null)
                {
                    continue;
                }

                switch (sqlException.Number)
                {
                    case 50020:
                    case 50041:
                        return new IssueRuleException(
                            "El proyecto no existe o está inactivo.",
                            sqlException);
                    case 50021:
                        return new UnauthorizedAccessException(
                            "Su usuario no está registrado como miembro "
                            + "del proyecto.", sqlException);
                    case 50022:
                        return new IssueRuleException(
                            "El responsable seleccionado no pertenece "
                            + "al proyecto.", sqlException);
                    case 50040:
                        return new IssueNotFoundException(sqlException);
                    case 50042:
                        return new UnauthorizedAccessException(
                            "No tiene permiso para cambiar el estado.",
                            sqlException);
                    case 50043:
                        return new IssueRuleException(
                            "La tarea ya tiene ese estado.", sqlException);
                    case 50044:
                        return new IssueRuleException(
                            "La transición desde el estado actual al "
                            + "seleccionado no está permitida.",
                            sqlException);
                    case 547:
                        // Conflicto de FK/CHECK: algún catálogo no es válido.
                        return new IssueRuleException(
                            "Uno de los valores seleccionados no es válido.",
                            sqlException);
                }
            }

            return null;
        }
    }

    // Columnas reales del SELECT final de dbo.usp_CreateIssue.
    internal sealed class CreatedIssueRow
    {
        public long IssueId { get; set; }
        public string IssueKey { get; set; }
        public string Title { get; set; }
    }

    // Columnas reales del SELECT final de dbo.usp_ChangeIssueStatus.
    internal sealed class StatusChangedRow
    {
        public long IssueId { get; set; }
        public string Title { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
