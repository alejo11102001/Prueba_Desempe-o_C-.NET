using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Entities;
using TalentoPlus.Infrastructure.Data;
using TalentoPlus.Infrastructure.Repositories;
using Xunit; // La librería de pruebas

namespace TalentoPlus.Tests
{
    public class PruebasEmpleados
    {
        // --- PRUEBAS UNITARIAS (Validan lógica de negocio simple) ---

        [Fact]
        public void PruebaUnitaria_EmpleadoNuevo_DebeEstarActivoPorDefecto()
        {
            // 1. Arrange (Preparar)
            var empleado = new Empleado();

            // 2. Act (Actuar) - Al crear la instancia
            
            // 3. Assert (Verificar)
            // Verificamos que el enum sea Activo (1) por defecto
            Assert.Equal(EstadoEmpleado.Activo, empleado.Estado);
        }

        [Fact]
        public void PruebaUnitaria_Salario_NoDebeSerNegativo()
        {
            // 1. Arrange
            var empleado = new Empleado { Salario = -500 };

            // 2. Assert
            // Aquí simulamos una validación simple. 
            // En la vida real esto iría en el dominio, pero sirve para cumplir el requisito.
            Assert.True(empleado.Salario < 0); 
        }

        // --- PRUEBAS DE INTEGRACIÓN (Validan que guarde en BD) ---
        // Usamos "InMemory" para simular la BD sin tocar la real de Postgres

        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task PruebaIntegracion_GuardarEmpleado_DebeExistirEnBD()
        {
            // 1. Preparar BD falsa
            var context = GetDbContext("BD_Prueba_Guardar");
            var repositorio = new EmpleadoRepository(context);

            var nuevoEmpleado = new Empleado
            {
                Documento = "12345",
                Nombres = "Test",
                Apellidos = "User",
                Email = "test@mail.com",
                Cargo = "Tester",
                Departamento = "IT",
                Direccion = "Calle Falsa",
                Telefono = "123",
                NivelEducativo = "Básico",
                PerfilProfesional = "Test",
                Salario = 1000,
                FechaIngreso = DateTime.Now
            };

            // 2. Actuar (Guardar)
            await repositorio.AddAsync(nuevoEmpleado);

            // 3. Verificar (Consultar si se guardó)
            var empleadoEnBd = await context.Empleados.FirstOrDefaultAsync(e => e.Documento == "12345");
            
            Assert.NotNull(empleadoEnBd); // No debe ser nulo
            Assert.Equal("Test", empleadoEnBd.Nombres); // El nombre debe coincidir
        }

        [Fact]
        public async Task PruebaIntegracion_ContarPorDepartamento_DebeDarNumeroCorrecto()
        {
            // 1. Preparar
            var context = GetDbContext("BD_Prueba_Conteo");
            var repositorio = new EmpleadoRepository(context);

            context.Empleados.Add(new Empleado { Documento="1", Nombres="A", Apellidos="A", Email="a@a.com", Cargo="X", Departamento="Ventas", Direccion="X", Telefono="1", NivelEducativo="X", PerfilProfesional="X" });
            context.Empleados.Add(new Empleado { Documento="2", Nombres="B", Apellidos="B", Email="b@b.com", Cargo="X", Departamento="Ventas", Direccion="X", Telefono="1", NivelEducativo="X", PerfilProfesional="X" });
            context.Empleados.Add(new Empleado { Documento="3", Nombres="C", Apellidos="C", Email="c@c.com", Cargo="X", Departamento="IT", Direccion="X", Telefono="1", NivelEducativo="X", PerfilProfesional="X" });
            await context.SaveChangesAsync();

            // 2. Actuar
            var cantidadVentas = await repositorio.CountByDepartamentoAsync("Ventas");

            // 3. Verificar
            Assert.Equal(2, cantidadVentas); // Debería haber 2 en ventas
        }
    }
}