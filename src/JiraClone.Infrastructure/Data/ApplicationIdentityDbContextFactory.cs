using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JiraClone.Infrastructure.Data
{
    // Permite crear el DbContext desde comandos de Entity Framework.
    public class ApplicationIdentityDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
    {
        // Construye el contexto para operaciones de diseño, como migraciones.
        public ApplicationIdentityDbContext CreateDbContext(string[] args)
        {
            // Configura el proveedor SQL Server sin abrir una conexión.
            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationIdentityDbContext>();

            // Esta conexión se utiliza para configurar el modelo.
            // No se ejecutará contra SQL Server al generar el script.
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=JiraCloneDB;Trusted_Connection=True;TrustServerCertificate=True;");

            // Devuelve una instancia del contexto con su configuración.
            return new ApplicationIdentityDbContext(optionsBuilder.Options);
        }
    }
}
