using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Cargo.Infrastructure.Repositories;
using Cargo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

// Это говорит EPPlus, что мы бедные студенты и не платим за лицензию
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

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

// Services
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

// Services
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

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
        var context = services.GetRequiredService<Cargo.Infrastructure.Data.CargoDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Применяем миграции
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
        
        // === SEED: Создаем тестового тенанта для MVP ===
        var mockTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!context.Tenants.Any(t => t.Id == mockTenantId))
        {
            context.Tenants.Add(new Cargo.Core.Entities.Tenant 
            { 
                Id = mockTenantId,
                TenantId = mockTenantId, // Сам себе тенант
                TenantCode = "TEST",
                CompanyName = "Test Cargo Company",
                ContactEmail = "test@cargo.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
            logger.LogInformation("Test tenant created with ID: {TenantId}", mockTenantId);
        }
        else
        {
            logger.LogInformation("Test tenant already exists");
        }
        // === END SEED ===
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
