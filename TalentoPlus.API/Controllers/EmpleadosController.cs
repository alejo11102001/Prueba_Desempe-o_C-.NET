using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TalentoPlus.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TalentoPlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- ¡EL GUARDIÁN! Solo entran tokens válidos
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoRepository _repository;

        public EmpleadosController(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Empleados/me
        // Devuelve los datos del empleado logueado (JSON)
        [HttpGet("me")]
        public async Task<IActionResult> GetMiInformacion()
        {
            // 1. Obtener el ID del usuario desde el Token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Buscar al empleado en la BD usando ese ID
            // (Nota: Necesitamos un método para buscar por UsuarioId, o filtramos aquí)
            var todos = await _repository.GetAllAsync();
            var empleado = todos.FirstOrDefault(e => e.UsuarioId == userId);

            if (empleado == null) return NotFound("No se encontró un perfil de empleado asociado a este usuario.");

            return Ok(empleado);
        }

        // GET: api/Empleados/me/pdf
        // Descarga la Hoja de Vida del empleado logueado
        [HttpGet("me/pdf")]
        public async Task<IActionResult> DescargarMiHojaVida()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var todos = await _repository.GetAllAsync();
            var empleado = todos.FirstOrDefault(e => e.UsuarioId == userId);

            if (empleado == null) return NotFound();

            // 3. Generar PDF (Copia de la lógica de la Web)
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Hoja de Vida: {empleado.Nombres} {empleado.Apellidos}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(10);
                            col.Item().Text($"Cargo: {empleado.Cargo}").Bold();
                            col.Item().Text($"Email: {empleado.Email}");
                            col.Item().Text($"Departamento: {empleado.Departamento}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().Text("Perfil Profesional").Bold();
                            col.Item().Text(empleado.PerfilProfesional ?? "Sin perfil");
                        });

                    page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
                });
            });

            var pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Mi_HV.pdf");
        }
    }
}