using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TalentoPlus.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly ApplicationDbContext _context;

        public EmpleadoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Empleado entity)
        {
            _context.Empleados.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.Empleados.FindAsync(id);
            if (emp != null)
            {
                _context.Empleados.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _context.Empleados.ToListAsync();
        }

        public async Task<Empleado> GetByIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task<Empleado> GetByDocumentoAsync(string documento)
        {
            return await _context.Empleados.FirstOrDefaultAsync(e => e.Documento == documento);
        }

        public async Task UpdateAsync(Empleado entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Métodos para el Dashboard/IA
        public async Task<int> CountByEstadoAsync(EstadoEmpleado estado)
        {
            return await _context.Empleados.CountAsync(e => e.Estado == estado);
        }

        public async Task<int> CountByDepartamentoAsync(string departamento)
        {
            // Usamos ToLower para evitar problemas de mayúsculas/minúsculas
            return await _context.Empleados
                .CountAsync(e => e.Departamento.ToLower() == departamento.ToLower());
        }
    }
}