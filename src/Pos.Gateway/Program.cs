using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// How the gateway works here:
// 1. The web app calls one address (this one) and never learns where the two APIs run.
// 2. ocelot.json maps an upstream path such as /pos/{everything} to a downstream API and path such as /api/{everything}.
// 3. The Authorization header travels through untouched; each API validates the token itself. No caching, no rate limiting, no gateway login.
// Follows the Ocelot docs "Getting started" page: https://ocelot.readthedocs.io/en/latest/introduction/gettingstarted.html
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Locally ocelot.json points at localhost ports; in Docker Compose ASPNETCORE_ENVIRONMENT=Docker picks ocelot.Docker.json, which points at the service names.
// One whole file is chosen on purpose: Ocelot's own AddOcelot() on the configuration builder would merge every ocelot.*.json in the folder into one route list.
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