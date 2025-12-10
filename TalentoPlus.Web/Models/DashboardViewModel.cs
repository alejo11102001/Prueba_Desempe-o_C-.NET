namespace TalentoPlus.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalEmpleados { get; set; }
        public int EmpleadosActivos { get; set; }
        public int EmpleadosEnVacaciones { get; set; }
        
        // Aquí guardaremos la respuesta de la IA cuando la usemos
        public string RespuestaIA { get; set; } 
    }
}