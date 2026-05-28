# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all project files first (layer-cache for restore)
COPY ShowSphere.sln .
COPY src/ShowSphere.API/ShowSphere.API.csproj src/ShowSphere.API/
COPY src/ShowSphere.Application/ShowSphere.Application.csproj src/ShowSphere.Application/
COPY src/ShowSphere.Domain/ShowSphere.Domain.csproj src/ShowSphere.Domain/
COPY src/ShowSphere.Infrastructure/ShowSphere.Infrastructure.csproj src/ShowSphere.Infrastructure/

RUN dotnet restore ShowSphere.sln

# Copy everything else and publish
COPY . .
RUN dotnet publish src/ShowSphere.API/ShowSphere.API.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render uses port 10000
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "ShowSphere.API.dll"]
