namespace JiraClone_EF6.Domain.Models
{
    /// <summary>
    /// Elemento de catálogo (tipo, prioridad o estado) para listas.
    /// </summary>
    public sealed class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Solo se usa en estados: TODO, IN_PROGRESS o DONE.
        public string Category { get; set; }
    }
}
