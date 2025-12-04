# 🚀 Быстрый старт Cargo.Solution

## Предварительные требования

- ✅ .NET 8 SDK установлен
- ✅ PostgreSQL 14+ установлен и запущен
- ✅ IDE (Visual Studio 2022, Rider или VS Code)

## Пошаговая инструкция

### Шаг 1: Установка зависимостей

```bash
cd /Users/daniillednik/CargoSaas
dotnet restore
```

### Шаг 2: Настройка базы данных

#### Создайте базу данных PostgreSQL:

```bash
createdb cargo_db
```

Или через SQL:

```sql
CREATE DATABASE cargo_db;
```

#### Настройте строку подключения

Откройте `src/Cargo.API/appsettings.json` и измените:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cargo_db;Username=ваш_юзер;Password=ваш_пароль"
  }
}
```

### Шаг 3: Создание и применение миграций

```bash
cd src/Cargo.API

# Создание миграции
dotnet ef migrations add InitialCreate --project ../Cargo.Infrastructure --startup-project .

# Применение миграции к БД
dotnet ef database update
```

### Шаг 4: Запуск приложения

```bash
dotnet run --project src/Cargo.API
```

Или из папки API:

```bash
cd src/Cargo.API
dotnet run
```

### Шаг 5: Открытие Swagger UI

После запуска откройте в браузере:

- **HTTPS**: https://localhost:5001/swagger
- **HTTP**: http://localhost:5000/swagger

## Тестирование API

### Создание тенанта

```bash
curl -X POST https://localhost:5001/api/tenants \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "companyName": "Моя Компания",
    "tenantCode": "my-company",
    "contactEmail": "info@mycompany.com",
    "contactPhone": "+7 999 123-45-67"
  }'
```

### Получение всех тенантов

```bash
curl -X GET https://localhost:5001/api/tenants -k
```

### Создание трека

**Важно**: Сначала установите TenantId в TenantProvider (это временное решение до внедрения JWT).

```bash
curl -X POST https://localhost:5001/api/tracks \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "clientCode": "CLIENT001",
    "trackingNumber": "TRACK123456789",
    "description": "Электроника из Китая",
    "weight": 5.5,
    "declaredValue": 15000,
    "originCountry": "Китай",
    "destinationCountry": "Россия",
    "estimatedDeliveryAt": "2025-01-15T00:00:00Z"
  }'
```

### Получение трека по номеру

```bash
curl -X GET https://localhost:5001/api/tracks/by-tracking-number/TRACK123456789 -k
```

## Проверка структуры проекта

```
Cargo.Solution/
├── src/
│   ├── Cargo.Core/              ✅ Domain Layer
│   │   ├── Entities/            - BaseEntity, Tenant, Track
│   │   └── Interfaces/          - IRepository, IUnitOfWork
│   │
│   ├── Cargo.Infrastructure/    ✅ Data Access Layer
│   │   ├── Data/                - CargoDbContext, TenantProvider
│   │   └── Repositories/        - Repository implementations
│   │
│   └── Cargo.API/               ✅ Presentation Layer
│       ├── Controllers/         - TenantsController, TracksController
│       ├── DTOs/                - Request/Response models
│       └── Program.cs           - DI configuration
│
├── Cargo.Solution.sln          ✅ Solution file
├── README.md                   ✅ Documentation
├── MIGRATION_GUIDE.md          ✅ EF Core migrations guide
└── QUICK_START.md             ✅ This file
```

## Полезные команды

### Компиляция проекта

```bash
dotnet build
```

### Очистка проекта

```bash
dotnet clean
```

### Запуск с hot reload

```bash
dotnet watch --project src/Cargo.API
```

### Проверка версии .NET

```bash
dotnet --version
```

### Проверка установленных инструментов EF

```bash
dotnet ef --version
```

## Возможные проблемы

### Ошибка: "Connection refused" при подключении к PostgreSQL

**Решение**: Убедитесь, что PostgreSQL запущен:

```bash
# macOS (Homebrew)
brew services start postgresql@16

# или
pg_ctl start

# Проверка статуса
brew services list
```

### Ошибка: "Build failed"

**Решение**: Проверьте версию .NET:

```bash
dotnet --version  # Должно быть 8.0.x
```

Если версия не подходит, установите .NET 8 SDK с официального сайта Microsoft.

### Ошибка: "dotnet-ef command not found"

**Решение**: Установите глобальный инструмент:

```bash
dotnet tool install --global dotnet-ef
```

### Ошибка: SSL certificate problem

**Решение**: Установите сертификат разработки:

```bash
dotnet dev-certs https --trust
```

## Следующие шаги

1. ✅ Проект создан и запущен
2. 🔲 Добавить JWT аутентификацию
3. 🔲 Реализовать middleware для извлечения TenantId из токена
4. 🔲 Добавить валидацию с FluentValidation
5. 🔲 Настроить логирование с Serilog
6. 🔲 Написать Unit-тесты
7. 🔲 Добавить пагинацию
8. 🔲 Настроить Docker

## Дополнительные ресурсы

- [Документация .NET 8](https://learn.microsoft.com/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Multi-tenancy patterns](https://learn.microsoft.com/azure/architecture/guide/multitenant/overview)

---

**Готово!** 🎉 Проект Cargo.Solution успешно настроен и готов к разработке!

