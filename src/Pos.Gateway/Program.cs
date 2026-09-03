using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// How the gateway works here:
// 1. The web app calls one address (this one) and never learns where the two APIs run.
// 2. ocelot.json maps an upstream path such as /pos/{everything} to a downstream API and path such as /api/{everything}.
// 3. The Authorization header travels through untouched; each API validates the token itself. No caching, no rate limiting, no gateway login.
// Follows the Ocelot docs "Getting started" page: https://ocelot.readthedocs.io/en/latest/introduction/gettingstarted.html
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Locally ocelot.json points at the two APIs on localhost ports. In Docker Compose the file ocelot.Docker.json points at the service names,
// and the compose file sets ASPNETCORE_ENVIRONMENT=Docker for the gateway so this line picks it. One whole file is chosen on purpose:
// Ocelot's own AddOcelot() on the configuration builder would merge every ocelot.*.json file in the folder into one route list.
string ocelotFileName = "ocelot.json";
if (builder.Environment.IsEnvironment("Docker"))
{
    ocelotFileName = "ocelot.Docker.json";
}

builder.Configuration.AddJsonFile(ocelotFileName, optional: false, reloadOnChange: false);
builder.Services.AddOcelot(builder.Configuration);

WebApplication app = builder.Build();

await app.UseOcelot();

await app.RunAsync();