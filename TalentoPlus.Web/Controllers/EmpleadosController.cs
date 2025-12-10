using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using QuestPDF.Fluent;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;
using QuestPDF.Helpers;       
using QuestPDF.Infrastructure;
using System.Text;
using System.Globalization;

namespace TalentoPlus.Web.Controllers
{
    [Authorize] // Solo entra el Administrador logueado
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoRepository _repository;

        // Inyección de dependencias
        public EmpleadosController(IEmpleadoRepository repository)
        {
            _repository = repository;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // --- LISTAR EMPLEADOS ---
        public async Task<IActionResult> Index()
        {
            var empleados = await _repository.GetAllAsync();
            return View(empleados);
        }

        // --- VISTA CARGA MASIVA ---
        public IActionResult CargaMasiva()
        {
            return View();
        }

        // --- PROCESAR EXCEL (LÓGICA CORREGIDA) ---
        [HttpPost]
        public async Task<IActionResult> ProcesarExcel(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                ViewBag.Error = "Por favor seleccione un archivo válido.";
                return View("CargaMasiva");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await archivoExcel.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var hoja = package.Workbook.Worksheets[0];
                        var filas = hoja.Dimension.Rows;

                        for (int fila = 2; fila <= filas; fila++)
                        {
                            var documento = hoja.Cells[fila, 1].Text;
                            if (string.IsNullOrEmpty(documento)) continue;

                            var empleadoExistente = await _repository.GetByDocumentoAsync(documento);
                            var empleado = empleadoExistente ?? new Empleado();

                            // Mapeo de columnas
                            empleado.Documento = documento.Trim();
                            empleado.Nombres = hoja.Cells[fila, 2].Text.Trim();
                            empleado.Apellidos = hoja.Cells[fila, 3].Text.Trim();
                            
                            if (DateTime.TryParse(hoja.Cells[fila, 4].Text, out DateTime feNac))
                                empleado.FechaNacimiento = feNac.ToUniversalTime();
                            
                            empleado.Direccion = hoja.Cells[fila, 5].Text;
                            empleado.Telefono = hoja.Cells[fila, 6].Text;
                            string emailCrudo = hoja.Cells[fila, 7].Text;

                            if (!string.IsNullOrEmpty(emailCrudo))
                            {
                                // 1. Quitar tildes (Gómez -> Gomez)
                                string sinTildes = RemoverTildes(emailCrudo);
    
                                // 2. Quitar espacios raros y convertir a minúsculas
                                empleado.Email = sinTildes.Replace(((char)160).ToString(), "") // Espacio Excel
                                    .Replace(" ", "")                    // Espacio normal
                                    .Trim()
                                    .ToLower();
                            }
                            empleado.Cargo = hoja.Cells[fila, 8].Text;
                            
                            if (decimal.TryParse(hoja.Cells[fila, 9].Text, out decimal salario))
                                empleado.Salario = salario;

                            if (DateTime.TryParse(hoja.Cells[fila, 10].Text, out DateTime feIng))
                                empleado.FechaIngreso = feIng.ToUniversalTime();
                            
                            string estadoTexto = hoja.Cells[fila, 11].Text.ToLower().Trim();

                            if (estadoTexto.Contains("vacaciones"))
                            {
                                empleado.Estado = EstadoEmpleado.Vacaciones;
                            }
                            else if (estadoTexto.Contains("inactivo") || estadoTexto.Contains("retirado"))
                            {
                                empleado.Estado = EstadoEmpleado.Inactivo;
                            }
                            else
                            {
                                empleado.Estado = EstadoEmpleado.Activo;
                            }
                            // ---------------------------------------------------

                            empleado.NivelEducativo = hoja.Cells[fila, 12].Text;
                            empleado.PerfilProfesional = hoja.Cells[fila, 13].Text;
                            empleado.Departamento = hoja.Cells[fila, 14].Text;

                            if (empleadoExistente == null)
                                await _repository.AddAsync(empleado);
                            else
                                await _repository.UpdateAsync(empleado);
                        }
                    }
                }
                
                TempData["Mensaje"] = "Carga masiva completada con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ocurrió un error procesando el archivo: {ex.Message}";
                return View("CargaMasiva");
            }
        }
        
        // --- ELIMINAR ---
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Mensaje"] = "Empleado eliminado correctamente.";
            return RedirectToAction("Index");
        }

        // --- DESCARGAR PDF ---
        public async Task<IActionResult> DescargarHojaVida(int id)
        {
            var empleado = await _repository.GetByIdAsync(id);
            if (empleado == null) return NotFound();

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
                            col.Item().Text($"Cargo Actual: {empleado.Cargo}").FontSize(14).Bold();
                            col.Item().Text($"Departamento: {empleado.Departamento}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);                            
                            col.Item().Text("Datos Personales").Bold();
                            col.Item().Text($"Documento: {empleado.Documento}");
                            col.Item().Text($"Email: {empleado.Email}");
                            col.Item().Text($"Teléfono: {empleado.Telefono}");
                            col.Item().Text($"Dirección: {empleado.Direccion}");
                            col.Item().Text($"Fecha Nacimiento: {empleado.FechaNacimiento:dd/MM/yyyy}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().Text("Información Laboral").Bold();
                            col.Item().Text($"Salario: {empleado.Salario:C}"); 
                            col.Item().Text($"Fecha Ingreso: {empleado.FechaIngreso:dd/MM/yyyy}");
                            col.Item().Text($"Estado Actual: {empleado.Estado}");
                            col.Item().Text($"Nivel Educativo: {empleado.NivelEducativo}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().Text("Perfil Profesional").Bold();
                            col.Item().Text(empleado.PerfilProfesional ?? "Sin perfil registrado").Justify();
                        });

                    page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
                });
            });

            var pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"HV_{empleado.Documento}.pdf");
        }
        
        // --- CREAR (GET) ---
        public IActionResult Crear()
        {
            CargarListasViewBag();
            return View();
        }

        // --- CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                empleado.FechaIngreso = empleado.FechaIngreso.ToUniversalTime();
                if (empleado.FechaNacimiento != default)
                    empleado.FechaNacimiento = empleado.FechaNacimiento.ToUniversalTime();

                await _repository.AddAsync(empleado);
                TempData["Mensaje"] = "Empleado creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            CargarListasViewBag(); 
            return View(empleado);
        }

        // --- EDITAR (GET) ---
        public async Task<IActionResult> Editar(int id)
        {
            var empleado = await _repository.GetByIdAsync(id);
            if (empleado == null) return NotFound();

            CargarListasViewBag();
            return View(empleado);
        }

        // --- EDITAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Empleado empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                empleado.FechaIngreso = empleado.FechaIngreso.ToUniversalTime();
                if (empleado.FechaNacimiento != default)
                    empleado.FechaNacimiento = empleado.FechaNacimiento.ToUniversalTime();

                await _repository.UpdateAsync(empleado);
                TempData["Mensaje"] = "Empleado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasViewBag();
            return View(empleado);
        }

        // --- MÉTODO AUXILIAR PARA LAS LISTAS ---
        private void CargarListasViewBag()
        {
            ViewBag.Departamentos = new List<string> 
            { 
                "Logística", "Marketing", "Recursos Humanos", "Operaciones", 
                "Ventas", "Tecnología", "Contabilidad" 
            };

            ViewBag.Cargos = new List<string> 
            { 
                "Ingeniero", "Soporte Técnico", "Analista", "Coordinador", 
                "Desarrollador", "Auxiliar", "Administrador" 
            };
            
            ViewBag.NivelesEducativos = new List<string>
            {
                "Bachiller", "Técnico", "Tecnólogo", "Profesional", "Especialista", "Maestría"
            };
        }
        // Función auxiliar para quitar tildes (á -> a, ñ -> n)
        private string RemoverTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";

            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}