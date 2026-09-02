using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// How the gateway works here:
// 1. The web app calls one address (this one) and never learns where the two APIs run.
// 2. ocelot.json maps an upstream path such as /pos/{everything} to a downstream API and path such as /api/{everything}.
// 3. The Authorization header travels through untouched; each API validates the token itself. No caching, no rate limiting, no gateway login.
// Follows the Ocelot docs "Getting started" page: https://ocelot.readthedocs.io/en/latest/introduction/gettingstarted.html
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddOcelot();
builder.Services.AddOcelot(builder.Configuration);

WebApplication app = builder.Build();

await app.UseOcelot();

await app.RunAsync();