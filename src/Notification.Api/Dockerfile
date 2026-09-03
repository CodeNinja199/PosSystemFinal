# The same two stages as the POS API's Dockerfile; this service is one project, so only its own folder is copied.
# Built from the repository root: docker build -f src/Notification.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Notification.Api/ src/Notification.Api/
RUN dotnet publish src/Notification.Api/Notification.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]