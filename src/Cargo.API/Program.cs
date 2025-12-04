using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Cargo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cargo API",
        Version = "v1",
        Description = "B2B SaaS платформа для отслеживания грузов с multi-tenancy"
    });
});

// Database configuration
// Поддержка Railway DATABASE_URL и локальной разработки
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway предоставляет DATABASE_URL в формате:
    // postgresql://user:password@host:port/database
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    connectionString = $"Host={databaseUri.Host};" +
                      $"Port={databaseUri.Port};" +
                      $"Database={databaseUri.LocalPath.TrimStart('/')};" +
                      $"Username={userInfo[0]};" +
                      $"Password={userInfo[1]};" +
                      $"SSL Mode=Require;" +
                      $"Trust Server Certificate=true";
}
else
{
    // Локальная разработка или custom connection string
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<CargoDbContext>(options =>
    options.UseNpgsql(connectionString));

// Tenant provider (будет расширен для извлечения из HTTP-заголовка)
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Cargo.Infrastructure.Data.CargoDbContext>(); // Проверь namespace контекста!
        context.Database.Migrate(); // Это применит все миграции к базе
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

