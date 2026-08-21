using Microsoft.EntityFrameworkCore;
using Pos.Api;
using Pos.Api.Middleware;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Infrastructure.Data;
using Pos.Infrastructure.Repositories;
using Pos.Infrastructure.SeedData;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ValidationErrorResponseFactory.CreateResponse;
    });

// Swagger setup follows the Swashbuckle.AspNetCore README "Getting Started" steps.
builder.Services.AddSwaggerGen();

// The connection string comes from user-secrets locally and from an environment variable in Docker; it is never in appsettings.json.
string? posDatabaseConnectionString = builder.Configuration.GetConnectionString("PosDatabase");
if (posDatabaseConnectionString == null)
{
    throw new InvalidOperationException("ConnectionStrings:PosDatabase is not configured.");
}

builder.Services.AddDbContext<PosDbContext>(options =>
{
    options.UseSqlServer(posDatabaseConnectionString);
});

// Application services: scoped, so one request shares one instance of each.
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();

// Infrastructure repositories: scoped, like the DbContext they hold.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<SeedDataLoader>();

WebApplication app = builder.Build();

// Fill an empty database once, before the API starts listening. The scope gives the loader its own DbContext.
using (IServiceScope seedScope = app.Services.CreateScope())
{
    SeedDataLoader seedDataLoader = seedScope.ServiceProvider.GetRequiredService<SeedDataLoader>();
    await seedDataLoader.LoadAsync();
}

// Configure the HTTP request pipeline. Error handling comes first so it wraps everything after it.
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();