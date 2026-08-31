using Microsoft.EntityFrameworkCore;

using Notification.Api.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

WebApplication app = builder.Build();

// Bring the database up to date before the API starts listening.
using (IServiceScope startupScope = app.Services.CreateScope())
{
    NotificationDbContext startupContext = startupScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await startupContext.Database.MigrateAsync();
}

app.UseAuthorization();

app.MapControllers();

app.Run();