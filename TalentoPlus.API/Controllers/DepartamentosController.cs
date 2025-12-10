using Microsoft.AspNetCore.Mvc;

namespace TalentoPlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentosController : ControllerBase
    {
        // GET: api/Departamentos
        // Endpoint PÚBLICO (No tiene [Authorize])
        [HttpGet]
        public IActionResult GetDepartamentos()
        {
            // Retornamos la misma lista oficial que definimos en la Web y el Excel.
            // Nota: Podríamos sacarla de la BD con un "Select Distinct", 
            // pero si la BD está vacía al inicio, nadie podría registrarse.
            // Por eso es mejor tener la lista maestra aquí.
            var departamentos = new List<string>
            {
                "Logística", 
                "Marketing", 
                "Recursos Humanos", 
                "Operaciones", 
                "Ventas", 
                "Tecnología", 
                "Contabilidad"
            };

            return Ok(departamentos);
        }
    }
}