using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentoPlus.Core.Entities
{
    public enum EstadoEmpleado
    {
        Activo = 1,
        Inactivo = 0,
        Vacaciones = 2
    }

    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Documento { get; set; } // Clave principal del negocio

        [Required]
        [MaxLength(50)]
        public string Nombres { get; set; }

        [Required]
        [MaxLength(50)]
        public string Apellidos { get; set; }

        public DateTime FechaNacimiento { get; set; } // Nuevo

        public string Direccion { get; set; } // Nuevo

        public string Telefono { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Cambiado de 'Correo' a 'Email' para coincidir con Excel

        [Required]
        public string Cargo { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Buena práctica para dinero en BD
        public decimal Salario { get; set; }

        public DateTime FechaIngreso { get; set; }

        public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

        public string NivelEducativo { get; set; }

        public string PerfilProfesional { get; set; } // Nuevo

        [Required]
        public string Departamento { get; set; }

        // Propiedad para vincular con el usuario del sistema (Identity) más adelante
        public string? UsuarioId { get; set; }
    }
}