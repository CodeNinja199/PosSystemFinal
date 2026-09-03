# The same two stages as the POS API's Dockerfile; this service is one project, so only its own folder is copied.
# Built from the repository root: docker build -f src/Pos.Gateway/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Pos.Gateway/ src/Pos.Gateway/
RUN dotnet publish src/Pos.Gateway/Pos.Gateway.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pos.Gateway.dll"]