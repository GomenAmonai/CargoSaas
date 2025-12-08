using System.Text;
using Cargo.API.Middleware;
using Cargo.Core;
using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Cargo.Infrastructure.Repositories;
using Cargo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;

// Это говорит EPPlus, что мы бедные студенты и не платим за лицензию
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// Configure Forwarded Headers для Railway (SSL терминируется на балансировщике)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cargo API",
        Version = "v1.0.0",
        Description = "🚀 B2B SaaS платформа для отслеживания грузов с multi-tenancy и Telegram WebApp\n\n" +
                      "✅ Telegram WebApp Authentication\n" +
                      "✅ ASP.NET Core Identity\n" +
                      "✅ Multi-tenancy\n" +
                      "✅ JWT Authentication"
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

// ASP.NET Core Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings (для Managers, если будут)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    
    // User settings
    options.User.RequireUniqueEmail = false; // Telegram users don't have email
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<CargoDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CargoAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CargoClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// HttpContextAccessor для доступа к HttpContext в сервисах
builder.Services.AddHttpContextAccessor();

// Tenant provider - извлекает TenantId из JWT claims
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Services
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddScoped<ITelegramAuthService, TelegramAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Hosted Services (Background Tasks)
builder.Services.AddHostedService<TelegramBotBackgroundService>();

// CORS configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" }; // Fallback для локальной разработки

// Добавляем домены Telegram WebApp для production (если их еще нет в конфиге)
var telegramWebAppOrigins = new[]
{
    "https://web.telegram.org",
    "https://webk.telegram.org",
    "https://webz.telegram.org"
};

// Объединяем все разрешенные домены (убираем дубликаты)
var allAllowedOrigins = allowedOrigins
    .Concat(telegramWebAppOrigins)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allAllowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Для JWT токенов
    });
    
    // Для разработки - более мягкая политика
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

var app = builder.Build();

// Логируем разрешенные CORS домены для отладки
var corsLogger = app.Services.GetRequiredService<ILogger<Program>>();
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
corsLogger.LogInformation("CORS configured with {Count} origins from config: {Origins}", 
    configuredOrigins.Length, string.Join(", ", configuredOrigins));
corsLogger.LogInformation("Total allowed origins (including Telegram): {Count}", allAllowedOrigins.Length);

// Auto-apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CargoDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Применяем миграции
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
        
        // === SEED: Создаем тестового тенанта для MVP ===
        var testTenantId = AppConstants.Tenants.TestTenantId;
        if (!context.Tenants.Any(t => t.Id == testTenantId))
        {
            context.Tenants.Add(new Cargo.Core.Entities.Tenant 
            { 
                Id = testTenantId,
                TenantId = testTenantId, // Сам себе тенант
                TenantCode = AppConstants.Tenants.TestTenantCode,
                CompanyName = "Test Cargo Company",
                ContactEmail = "test@cargo.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
            logger.LogInformation("Test tenant created with ID: {TenantId}", testTenantId);
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
// ВАЖНО: UseForwardedHeaders ПЕРВЫМ для корректной работы на Railway
app.UseForwardedHeaders();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

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

// ОТКЛЮЧЕНО: На Railway SSL терминируется на балансировщике, 
// UseHttpsRedirection вызывает 405 Method Not Allowed для POST запросов
// app.UseHttpsRedirection();

// CORS: для production используем белый список, для dev - разрешаем все
// ВАЖНО: CORS должен быть ДО UseAuthentication и UseAuthorization
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins";
app.UseCors(corsPolicy);

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
