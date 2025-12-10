# 🚀 TalentoPlus — Sistema de Gestión de Empleados

🧩 **ASP.NET Core 8** • 🗄 **PostgreSQL** • 🔐 **Identity + JWT**  
🏗 **Clean Architecture & Repository Pattern** • 🧪 **xUnit Tests**  
🐳 **Docker & Docker Compose**

TalentoPlus es un sistema desarrollado para la **gestión de empleados**, control de acceso por roles, administración vía Web MVC e integración mediante **API protegida con tokens JWT**.

El proyecto se desarrolla siguiendo principios de **arquitectura limpia** garantizando desacoplamiento, mantenibilidad y escalabilidad.

---

## 🛠 Tecnologías Utilizadas

| Tecnología | Uso |
|-----------|-----|
| ASP.NET Core 8 MVC | Interfaz Web + Identity |
| ASP.NET Core 8 Web API | Endpoints protegidos JWT |
| Entity Framework Core | ORM / Migraciones |
| PostgreSQL | Base de datos |
| Clean Architecture | Separación de capas |
| Repository Pattern | Acceso a datos |
| Docker / Docker Compose | Contenedores |
| Swagger | Documentación API |
| xUnit | Pruebas |
| Coverlet | Cobertura de código |
| Moq | Mocking en pruebas |

---

## 📂 Estructura del Proyecto (Clean Architecture)

TalentoPlusSolution
│
├── TalentoPlus.Web → App MVC (Front + Identity)
├── TalentoPlus.API → API con JWT
├── TalentoPlus.Core → Dominio & Interfaces
├── TalentoPlus.Infrastructure → Repositorios & EF Core (migrations aquí)
├── TalentoPlus.Tests → Proyecto de pruebas unitarias y de integración
└── docker-compose.yml


✔ Presentación — Dominio — Infraestructura totalmente separadas  
✔ Repositorios desacoplados mediante inyección de dependencias  
✔ Entity Framework + PostgreSQL  
✔ Identity para Web + JWT en API  

---

## 🐳 Despliegue con Docker — **Pasos para correr la solución**

### 💡 Requisitos previos:
- Docker
- Docker Compose

### 📍 Desde la raíz del proyecto ejecutar:


- docker-compose build
- docker-compose up -d

### 🔍 Validar contenedores:

docker ps

---

## 🌐 Acceso a servicios
Servicio	URL
- Web MVC	http://localhost:8080
- API Swagger	http://localhost:8081/swagger
- PostgreSQL	localhost:5432

---

## 🔧 Configuración de Variables de Entorno
### PostgreSQL

- POSTGRES_USER=envyguard_user
- POSTGRES_PASSWORD=jE15QhCwINzUNUw1FdclOB8YqZOE89
- POSTGRES_DB=TalentoPlusDB-Diego

## ConnectionString para Docker interno

ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=TalentoPlusDB-Diego;Username=envyguard_user;Password=jE15QhCwINzUNUw1FdclOB8YqZOE89

### Web debe apuntar a la API dentro de Docker

API_URL=http://talentoplus_api:8081

### Si conectas desde tu máquina local:

Host=localhost;Port=5433;...

---

## 🔐 Credenciales de Acceso

### 💻 Login Web (Administrador por defecto)

#### Usuario

- admin@talentoplus.com
  
#### Contraseña

- Admin123!

✔ Se crea automáticamente al iniciar

---

## 🔑 Autenticación API (JWT)

### POST a:

/api/Auth/login

### Body:

{
  "email": "admin@talentoplus.com",
  "password": "Admin123!"
}

### Header:

Authorization: Bearer <token>

---

## 🧪 Pruebas (.Tests)
### 1️⃣ Ejecutar pruebas locales

dotnet test ./TalentoPlus.Tests/TalentoPlus.Tests.csproj

### O para toda la solución:

dotnet test

### 2️⃣ Tests con cobertura

dotnet test ./TalentoPlus.Tests/TalentoPlus.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.xml

### 3️⃣ Ejecutar tests dentro de Docker

docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -c "dotnet test ./TalentoPlus.Tests/TalentoPlus.Tests.csproj --logger 'trx;LogFileName=test_results.trx'"

### 4️⃣ CI con GitHub Actions (ejemplo)

Archivo: .github/workflows/dotnet-test.yml

name: .NET Tests
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore --configuration Release
      - name: Test
        run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"

---

## 📜 Git — Subir proyecto al repositorio

- git init
- dotnet new gitignore
- git add .
- git commit -m "Initial commit - TalentoPlusSolution with tests"
- git remote add origin https://github.com/alejo11102001/Prueba_Desempe-o_C-.NET.git
- git branch -M main
- git push -u origin main

---

## 🌐 Link del Repositorio

🔗 https://github.com/alejo11102001/Prueba_Desempe-o_C-.NET.git

---

## 🧾 Resumen de comandos útiles

docker compose build
docker compose up -d
docker compose down
docker ps
docker logs -f talentoplus_api
dotnet test
docker exec -it talentoplus_api bash

---

## 🧑‍💻 Mantenimiento y notas finales

    Migraciones en: TalentoPlus.Infrastructure/Migrations

    El seeding del admin se ejecuta en arranque

    No subir contraseñas ni appsettings.json sensibles

## 🧑‍💻 Desarrollador

Diego Alejandro Zuluaga Yepes
Pruebra de desempeño — Ruta Avanzada .NET
Van rossum
