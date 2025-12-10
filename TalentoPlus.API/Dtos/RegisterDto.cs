using System.ComponentModel.DataAnnotations;

namespace TalentoPlus.API.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Documento { get; set; }

        [Required]
        public string Nombres { get; set; }
        
        [Required]
        public string Apellidos { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}