using System;
using System.Linq;
using System.Threading.Tasks;
using JiraClone_EF6.Application.DTOs.Comments;
using JiraClone_EF6.Application.DTOs.Common;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Interfaces;
namespace JiraClone_EF6.Application.Services
{
    public sealed class IssueCommentService : IIssueCommentService
    {
        private readonly IIssueCommentRepository _repository;
        private readonly IProjectAccessService _access;
        public IssueCommentService(IIssueCommentRepository repository, IProjectAccessService access)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _access = access ?? throw new ArgumentNullException(nameof(access));
        }
        public async Task<PagedResult<IssueCommentDto>> GetByIssueAsync(
            int projectId, long issueId, string userId, int page, int pageSize)
        {
            ValidateIds(projectId, issueId, userId);
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(page), "Página o tamaño inválido.");
            if (!await _access.CanViewAsync(projectId, userId))
                throw new UnauthorizedAccessException("No tiene acceso a este proyecto.");
            if (!await _repository.IssueBelongsToProjectAsync(projectId, issueId))
                throw new ArgumentException("La tarea no pertenece al proyecto.");
            // Se solicita un elemento adicional para conocer si hay otra página.
            int skip = checked((page - 1) * pageSize);
            var rows = await _repository.GetByIssueAsync(issueId, skip, pageSize + 1);
            return new PagedResult<IssueCommentDto>
            {
                Items = rows.Take(pageSize).Select(c => new IssueCommentDto
                {
                    IssueCommentId = c.IssueCommentId,
                    IssueId = c.IssueId,
                    AuthorUserId = c.AuthorUserId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    IsEdited = c.IsEdited
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                HasNextPage = rows.Count > pageSize
            };
        }
        public async Task<long> AddAsync(int projectId, long issueId, string content, string userId)
        {
            ValidateIds(projectId, issueId, userId);
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Escriba un comentario.", nameof(content));
            if (!await _access.CanWriteAsync(projectId, userId))
                throw new UnauthorizedAccessException("No tiene permiso para comentar.");
            if (!await _repository.IssueBelongsToProjectAsync(projectId, issueId))
                throw new ArgumentException("La tarea no pertenece al proyecto.");
            return await _repository.AddAsync(issueId, content.Trim(), userId);
        }
        private static void ValidateIds(int projectId, long issueId, string userId)
        {
            if (projectId <= 0 || issueId <= 0 || string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Proyecto, tarea o usuario inválido.");
        }
    }
}
