# Multi-stage build from the Microsoft docs page "Containerize a .NET app": the sdk image compiles, the smaller aspnet image runs.
# Built from the repository root so the four POS projects are in the context: docker build -f src/Pos.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Pos.Domain/ src/Pos.Domain/
COPY src/Pos.Application/ src/Pos.Application/
COPY src/Pos.Infrastructure/ src/Pos.Infrastructure/
COPY src/Pos.Api/ src/Pos.Api/
RUN dotnet publish src/Pos.Api/Pos.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
# curl is here so Docker's healthcheck can ask the app whether it is really answering,
# which is what puts the green 'healthy' badge next to the container in Docker Desktop.
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pos.Api.dll"]