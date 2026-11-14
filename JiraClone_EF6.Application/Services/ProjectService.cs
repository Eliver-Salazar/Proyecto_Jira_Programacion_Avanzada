using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JiraClone_EF6.Application.Interfaces;
using JiraClone_EF6.Domain.Exceptions;
using JiraClone_EF6.Domain.Interfaces;
using JiraClone_EF6.Domain.Models;

namespace JiraClone_EF6.Application.Services
{
    /// <summary>
    /// Coordina permisos, validaciones y operaciones de proyectos.
    /// </summary>
    public sealed class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAccessService _accessService;

        public ProjectService(
            IProjectRepository projectRepository,
            IProjectAccessService accessService)
        {
            _projectRepository = projectRepository
                ?? throw new ArgumentNullException(nameof(projectRepository));

            _accessService = accessService
                ?? throw new ArgumentNullException(nameof(accessService));
        }

        public async Task<ProjectSummary> GetByIdAsync(
            int projectId, string userId)
        {
            // El detalle se consulta únicamente después de comprobar el acceso.
            if (!await _accessService.CanViewAsync(projectId, userId))
            {
                return null;
            }

            var project = await _projectRepository.GetByIdAsync(projectId);

            return project != null && project.IsActive ? project : null;
        }

        public Task<List<ProjectSummary>> GetForUserAsync(
            string userId, int page)
        {
            ValidateUser(userId);

            if (page < 1 || page > 100000)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            // Se muestran 20 registros; el adicional indica si hay otra página.
            return _projectRepository.GetForUserAsync(
                userId, (page - 1) * 20, 21);
        }

        public async Task<int> CreateAsync(
            CreateProjectRequest request, string userId)
        {
            ValidateUser(userId);

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Normaliza los datos antes de validarlos y guardarlos.
            var normalized = new CreateProjectRequest
            {
                ProjectKey = (request.ProjectKey ?? "").Trim().ToUpperInvariant(),
                Name = (request.Name ?? "").Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim()
            };

            // Convención de la aplicación para identificadores como APP-1.
            if (!Regex.IsMatch(
                normalized.ProjectKey, @"\A[A-Z][A-Z0-9]{1,19}\z"))
            {
                throw new ArgumentException(
                    "La clave debe tener entre 2 y 20 caracteres, "
                    + "empezar con una letra y contener solo letras y números.");
            }

            if (normalized.Name.Length == 0 || normalized.Name.Length > 150)
            {
                throw new ArgumentException(
                    "El nombre es obligatorio y admite hasta 150 caracteres.");
            }

            if (normalized.Description != null
                && normalized.Description.Length > 1000)
            {
                throw new ArgumentException(
                    "La descripción admite hasta 1000 caracteres.");
            }

            // Mejora el mensaje al usuario; SQL sigue garantizando unicidad.
            if (await _projectRepository.KeyExistsAsync(normalized.ProjectKey))
            {
                throw new DuplicateProjectKeyException();
            }

            return await _projectRepository.CreateAsync(normalized, userId);
        }

        private static void ValidateUser(string userId)
        {
            // La creación requiere una identidad autenticada.
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Debe iniciar sesión para acceder a los proyectos.");
            }
        }
    }
}