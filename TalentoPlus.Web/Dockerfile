FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TalentoPlus.Web/TalentoPlus.Web.csproj", "TalentoPlus.Web/"]
COPY ["TalentoPlus.API/TalentoPlus.API.csproj", "TalentoPlus.API/"]
COPY ["TalentoPlus.Core/TalentoPlus.Core.csproj", "TalentoPlus.Core/"]
COPY ["TalentoPlus.Infrastructure/TalentoPlus.Infrastructure.csproj", "TalentoPlus.Infrastructure/"]
RUN dotnet restore "TalentoPlus.Web/TalentoPlus.Web.csproj"
COPY . .
WORKDIR "/src/TalentoPlus.Web"
RUN dotnet build "TalentoPlus.Web.csproj" -c Release -o /app/build
RUN dotnet publish "TalentoPlus.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TalentoPlus.Web.dll"]
