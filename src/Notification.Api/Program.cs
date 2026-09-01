using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Notification.Api.Data;
using Notification.Api.Messaging;
using Notification.Api.Middleware;
using Notification.Api.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger setup follows the Swashbuckle.AspNetCore README ("Getting Started" and the bearer security definition that adds the Authorize button).
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from the POS API's POST api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
    });
});

// One token, two services: the same Jwt settings as the POS API, checked the same way, with the algorithm pinned to HS256.
JwtSettings? jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings == null || jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret must be configured with at least 32 characters.");
}

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

// The connection string comes from user-secrets locally and from an environment variable in Docker.
string? notificationDatabaseConnectionString = builder.Configuration.GetConnectionString("NotificationDatabase");
if (notificationDatabaseConnectionString == null)
{
    throw new InvalidOperationException("ConnectionStrings:NotificationDatabase is not configured.");
}

builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseSqlServer(notificationDatabaseConnectionString);
});

// The RabbitMq section: only the host name. The consumer runs for the life of the app as a hosted service.
RabbitMqSettings? rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
if (rabbitMqSettings == null || rabbitMqSettings.Host.Length == 0)
{
    throw new InvalidOperationException("RabbitMq:Host must be configured.");
}

builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddHostedService<NotificationMessagesConsumer>();

WebApplication app = builder.Build();

// Error handling comes first so it wraps everything after it.
app.UseMiddleware<ErrorHandlingMiddleware>();

// Bring the database up to date before the API starts listening.
using (IServiceScope startupScope = app.Services.CreateScope())
{
    NotificationDbContext startupContext = startupScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await startupContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();