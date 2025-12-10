FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TalentoPlus.API/TalentoPlus.API.csproj", "TalentoPlus.API/"]
COPY ["TalentoPlus.Infrastructure/TalentoPlus.Infrastructure.csproj", "TalentoPlus.Infrastructure/"]
COPY ["TalentoPlus.Core/TalentoPlus.Core.csproj", "TalentoPlus.Core/"]

RUN dotnet restore "TalentoPlus.API/TalentoPlus.API.csproj"

COPY . .
WORKDIR "/src/TalentoPlus.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TalentoPlus.API.dll"]
