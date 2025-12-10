using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Entities;

namespace TalentoPlus.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext // Hereda de Identity para manejar usuarios
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empleado> Empleados { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configuraciones adicionales si fueran necesarias
            builder.Entity<Empleado>()
                .HasIndex(e => e.Documento)
                .IsUnique(); // El documento no se repite
        }
    }
}