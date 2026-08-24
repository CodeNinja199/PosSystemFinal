using System.Text;
using System.Text.Json.Serialization;

using Azure.Identity;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Pos.Api;
using Pos.Api.Middleware;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Infrastructure.Data;
using Pos.Infrastructure.Repositories;
using Pos.Infrastructure.Security;
using Pos.Infrastructure.SeedData;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// How Key Vault works here:
// 1. When KeyVault:Url is configured (never locally; user-secrets holds the local values), the vault becomes one more configuration source,
//    read after appsettings.json and user-secrets, so its values win.
// 2. Secret names use -- for the colon (Jwt--Secret lands on Jwt:Secret), so nothing else in the app changes.
// 3. DefaultAzureCredential signs in as the developer (Azure CLI) or as the deployed app (managed identity); no key is stored anywhere.
// Learned from: https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration
string? keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (keyVaultUrl != null)
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ValidationErrorResponseFactory.CreateResponse;
    })
    // Enums travel as their names ("Card", "Placed"), not as numbers, in both directions.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger setup follows the Swashbuckle.AspNetCore README: "Getting Started" plus the bearer security definition
// that adds the Authorize button, so every endpoint can be tried with a token.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from POST api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
    });
});

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

// The Jwt section: Issuer and Audience from appsettings.json, Secret from user-secrets. One object for the whole app.
JwtSettings? jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings == null || jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret must be configured with at least 32 characters.");
}

// Token validation settings follow the Microsoft docs page on JWT bearer authentication in ASP.NET Core.
// ValidAlgorithms pins HS256 so a token signed any other way is rejected (the "algorithm confusion" attack).
byte[] jwtSecretBytes = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSecretBytes),
            ValidAlgorithms = new List<string> { SecurityAlgorithms.HmacSha256 }
        };
    });

// Application services: scoped, so one request shares one instance of each.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReportService>();

// Infrastructure repositories: scoped, like the DbContext they hold.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<SeedDataLoader>();

// Infrastructure tools: the hasher keeps no state, so one instance can serve every request.
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton(jwtSettings);

// The Seed section: the two staff passwords from user-secrets, read once.
SeedSettings? seedSettings = builder.Configuration.GetSection("Seed").Get<SeedSettings>();
if (seedSettings == null || seedSettings.AdminPassword.Length < 8 || seedSettings.CashierPassword.Length < 8)
{
    throw new InvalidOperationException("Seed:AdminPassword and Seed:CashierPassword must be configured with at least 8 characters.");
}

builder.Services.AddSingleton(seedSettings);
builder.Services.AddSingleton<ILoginTokenCreator, JwtLoginTokenCreator>();

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

// Authentication reads the token and sets User; authorization then checks [Authorize], so this order is fixed.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();