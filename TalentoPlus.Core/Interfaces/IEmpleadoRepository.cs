using TalentoPlus.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TalentoPlus.Core.Interfaces
{
    public interface IEmpleadoRepository
    {
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado> GetByIdAsync(int id);
        Task<Empleado> GetByDocumentoAsync(string documento);
        Task<int> AddAsync(Empleado entity);
        Task UpdateAsync(Empleado entity);
        Task DeleteAsync(int id);
        // Métodos específicos para el dashboard
        Task<int> CountByEstadoAsync(EstadoEmpleado estado);
        Task<int> CountByDepartamentoAsync(string departamento);
    }
}