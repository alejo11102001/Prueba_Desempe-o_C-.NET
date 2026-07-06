# 👥 TalentoPlus

Sistema fullstack de gestión de empleados con carga masiva de datos, dashboard de estadísticas, EmailService y pruebas unitarias. Desarrollado con arquitectura en capas y desplegado con Docker.

---

## 🏗️ Arquitectura

```
TalentoPlus/
├── TalentoPlus.API          # Web API — autenticación, empleados, departamentos
├── TalentoPlus.Core         # Entidades, interfaces, servicios de dominio
├── TalentoPlus.Infrastructure # DbContext, repositorios, EmailService, migraciones
├── TalentoPlus.Web          # Frontend MVC — vistas Razor, controllers
├── TalentoPlus.Tests        # Pruebas unitarias con xUnit
└── docker-compose.yml       # Orquestación de servicios
```

---

## ⚙️ Tech Stack

**Backend**
- C# .NET — Web API + MVC
- Entity Framework Core
- JWT Authentication
- MailKit — Envío de correos
- xUnit — Pruebas unitarias

**Frontend**
- ASP.NET Core MVC
- Razor Views
- Bootstrap 5

**Infraestructura**
- PostgreSQL
- Docker + Docker Compose

---

## 📋 Funcionalidades

- ✅ Autenticación con JWT (registro e inicio de sesión)
- ✅ CRUD de empleados con validaciones
- ✅ Carga masiva de empleados
- ✅ Gestión de departamentos
- ✅ Dashboard con estadísticas
- ✅ Servicio de correo electrónico
- ✅ Pruebas unitarias

---

## 🚀 Cómo correrlo localmente

### Con Docker (recomendado)
```bash
git clone https://github.com/alejo11102001/Prueba_Desempe-o_C-.NET.git
cd Prueba_Desempe-o_C-.NET
docker-compose up --build
```

Servicios disponibles:
- API: `http://localhost:5000`
- Web: `http://localhost:8080`
- PostgreSQL: `localhost:5432`

### Sin Docker
```bash
# Backend API
cd TalentoPlus.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend Web (otra terminal)
cd TalentoPlus.Web
dotnet run
```

---

## 🧪 Pruebas

```bash
cd TalentoPlus.Tests
dotnet test
```

---

## 👤 Autor

**Diego Alejandro Zuluaga Yepes**
- GitHub: [@alejo11102001](https://github.com/alejo11102001)
- LinkedIn: [diego-zuluaga-yepes](https://linkedin.com/in/diego-zuluaga-yepes-239137272)
- Portfolio: [alejo11102001.github.io/Portafolio](https://alejo11102001.github.io/Portafolio)
