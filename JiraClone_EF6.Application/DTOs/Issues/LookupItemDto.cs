namespace JiraClone_EF6.Application.DTOs.Issues
{
    /// <summary>
    /// Opción de catálogo para listas desplegables (tipo, prioridad, estado).
    /// </summary>
    public sealed class LookupItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Solo en estados: TODO, IN_PROGRESS o DONE.
        public string Category { get; set; }
    }
}
