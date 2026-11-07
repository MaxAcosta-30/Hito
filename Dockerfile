# API de Hito. Render construye esta imagen desde el repositorio.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: la restauración queda en caché mientras no cambien las dependencias.
COPY global.json ./
COPY src/Hito.Domain/Hito.Domain.csproj src/Hito.Domain/
COPY src/Hito.Application/Hito.Application.csproj src/Hito.Application/
COPY src/Hito.Infrastructure/Hito.Infrastructure.csproj src/Hito.Infrastructure/
COPY src/Hito.Api/Hito.Api.csproj src/Hito.Api/
RUN dotnet restore src/Hito.Api/Hito.Api.csproj -r linux-x64 -p:PublishReadyToRun=true

COPY src/ src/
# ReadyToRun: el código llega precompilado y casi no se usa el JIT al arrancar. En Render Free (0.1 de CPU)
# el JIT dominaba el arranque en frío y las primeras peticiones (medido en docs/proyecto/09-rendimiento.md).
RUN dotnet publish src/Hito.Api/Hito.Api.csproj -c Release -o /app --no-restore -r linux-x64 --self-contained false -p:PublishReadyToRun=true

# chiseled: sin shell ni gestor de paquetes y con usuario sin privilegios. "extra" agrega ICU y zonas horarias.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Hito.Api.dll"]
