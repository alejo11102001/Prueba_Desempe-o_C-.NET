using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization; // Necesario para leer el JSON de Google
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Web.Models;

namespace TalentoPlus.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IEmpleadoRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public DashboardController(IEmpleadoRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _repository.GetAllAsync();
            var modelo = new DashboardViewModel
            {
                TotalEmpleados = empleados.Count(),
                EmpleadosActivos = empleados.Count(e => e.Estado == EstadoEmpleado.Activo),
                EmpleadosEnVacaciones = empleados.Count(e => e.Estado == EstadoEmpleado.Vacaciones),
                // Recuperamos la respuesta si viene de una redirección
                RespuestaIA = TempData["RespuestaIA"] as string 
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> ConsultarIA(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta)) 
                return RedirectToAction("Index");

            string rawResponse = ""; // Para depuración

            try
            {
                // 1. Validar API Key
                var apiKey = _configuration["GoogleApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    TempData["RespuestaIA"] = "ERROR: No encontré la 'GoogleApiKey' en appsettings.json";
                    return RedirectToAction("Index");
                }

                // 2. Prompt blindado
                var prompt = $@"
                    Eres un asistente SQL para RRHH. Analiza la intención del usuario.
                    Base de datos: Departamentos (Logística, Marketing, RRHH, etc), Estados (Activo, Inactivo, Vacaciones).
                    
                    Tu respuesta debe ser ESTRICTAMENTE uno de estos formatos (sin texto extra, sin markdown):
                    COUNT_DEPT|NombreDepartamento
                    COUNT_STATUS|Estado
                    TOTAL|
                    UNKNOWN|

                    Usuario: '{pregunta}'
                ";

                // 3. Llamada a Gemini
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, jsonContent);
                rawResponse = await response.Content.ReadAsStringAsync(); // Guardamos lo que dice Google

                // Si Google devuelve error (ej: API Key mala)
                if (!response.IsSuccessStatusCode)
                {
                    TempData["RespuestaIA"] = $"Error de Google ({response.StatusCode}): {rawResponse}";
                    return RedirectToAction("Index");
                }

                // 4. Leer respuesta y LIMPIARLA
                var jsonNode = JsonNode.Parse(rawResponse);
                var respuestaIA = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (string.IsNullOrEmpty(respuestaIA))
                {
                    TempData["RespuestaIA"] = "La IA devolvió una respuesta vacía.";
                    return RedirectToAction("Index");
                }

                // Limpieza agresiva: quitamos espacios, saltos de linea y comillas raras
                respuestaIA = respuestaIA.Trim().Replace("\n", "").Replace("\r", "").Replace("```", "").Replace("*", "");

                // 5. Lógica de Negocio
                string resultadoFinal = "No pude interpretar la respuesta: " + respuestaIA;

                if (respuestaIA.Contains("|"))
                {
                    var partes = respuestaIA.Split('|');
                    var comando = partes[0].Trim().ToUpper();
                    var valor = partes.Length > 1 ? partes[1].Trim() : "";

                    switch (comando)
                    {
                        case "COUNT_DEPT":
                            // Busqueda flexible (contiene texto)
                            var todosDept = await _repository.GetAllAsync();
                            int countDept = todosDept.Count(e => e.Departamento.ToLower().Contains(valor.ToLower()));
                            resultadoFinal = $"Encontré {countDept} empleados en el departamento '{valor}'.";
                            break;

                        case "COUNT_STATUS":
                            EstadoEmpleado estadoEnum = EstadoEmpleado.Activo;
                            if (valor.ToLower().Contains("inact")) estadoEnum = EstadoEmpleado.Inactivo;
                            if (valor.ToLower().Contains("vaca")) estadoEnum = EstadoEmpleado.Vacaciones;

                            int countState = await _repository.CountByEstadoAsync(estadoEnum);
                            resultadoFinal = $"Hay {countState} empleados con estado '{valor}'.";
                            break;

                        case "TOTAL":
                            var todos = await _repository.GetAllAsync();
                            resultadoFinal = $"Total de empleados registrados: {todos.Count()}.";
                            break;
                        
                        case "UNKNOWN":
                            resultadoFinal = "La IA dice que no entiende la pregunta en el contexto de empleados.";
                            break;
                    }
                }

                TempData["RespuestaIA"] = resultadoFinal;
            }
            catch (Exception ex)
            {
                // Si falla, mostramos el error técnico para saber qué pasó
                TempData["RespuestaIA"] = $"Error Técnico: {ex.Message} | Respuesta Raw: {rawResponse}";
            }

            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> TestModelos()
        {
            var apiKey = _configuration["GoogleApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
    
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
    
            return Content(json, "application/json");
        }
    }
}