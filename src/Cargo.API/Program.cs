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
// Railway предоставляет DATABASE_URL, локально используем appsettings.json
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway DATABASE_URL формат: postgresql://user:password@host:port/database
    try
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':', 2);
        
        if (userInfo.Length < 2)
        {
            throw new InvalidOperationException("DATABASE_URL is malformed: missing username or password");
        }
        
        connectionString = $"Host={databaseUri.Host};" +
                          $"Port={databaseUri.Port};" +
                          $"Database={databaseUri.LocalPath.TrimStart('/')};" +
                          $"Username={userInfo[0]};" +
                          $"Password={userInfo[1]};" +
                          $"SSL Mode=Require;" +
                          $"Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to parse DATABASE_URL", ex);
    }
}
else
{
    // Локальная разработка - используем appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string not found");
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
// Swagger нужен всегда, пока мы тестируем
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cargo API V1");
    c.RoutePrefix = string.Empty; // Чтобы открывался сразу на корневом домене
});

// А вот DeveloperExceptionPage - только в деве
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
