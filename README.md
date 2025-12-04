# Cargo.Solution - B2B SaaS платформа для отслеживания грузов

## 📋 Описание проекта

Cargo.Solution - это B2B SaaS решение для отслеживания грузов с поддержкой multi-tenancy. Платформа позволяет различным компаниям управлять своими треками и грузами в изолированной среде.

## 🏗️ Архитектура

Проект построен на основе **Clean Architecture** с разделением на слои:

```
Cargo.Solution/
├── src/
│   ├── Cargo.Core/              # Ядро приложения (Entities, Interfaces)
│   ├── Cargo.Infrastructure/    # Инфраструктура (DbContext, Repositories)
│   └── Cargo.API/               # API слой (Controllers, DTOs)
```

### Слои архитектуры:

- **Cargo.Core**: Бизнес-логика, сущности, интерфейсы репозиториев
- **Cargo.Infrastructure**: Реализация доступа к данным, EF Core, репозитории
- **Cargo.API**: REST API, контроллеры, DTOs, конфигурация DI

## 🛠️ Технологический стек

- **.NET 8** - Основной фреймворк
- **ASP.NET Core Web API** - REST API
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - База данных
- **Swagger/OpenAPI** - Документация API

## 🔑 Основные возможности

### Multi-tenancy

Проект полностью поддерживает multi-tenancy через:
- Колонку `TenantId` во всех сущностях
- Глобальные фильтры запросов в EF Core
- Автоматическую изоляцию данных между тенантами

### Сущности

**BaseEntity** - базовый класс для всех сущностей:
- `Id` - уникальный идентификатор (Guid)
- `TenantId` - идентификатор тенанта (Guid)
- `CreatedAt` - дата создания
- `UpdatedAt` - дата обновления

**Tenant** - сущность компании/организации:
- Название компании
- Уникальный код тенанта
- Контактная информация
- Статус активности
- Дата истечения подписки

**Track** - сущность отслеживаемого груза:
- Код клиента
- Трек-номер
- Статус (Created, InTransit, Delivered, и т.д.)
- Описание, вес, стоимость
- Страны отправления и назначения
- Даты отправки и доставки

## 🚀 Быстрый старт

### Предварительные требования

- .NET 8 SDK
- PostgreSQL 14+
- IDE (Visual Studio 2022, Rider или VS Code)

### Установка и запуск

1. **Клонируйте репозиторий**:
```bash
cd /path/to/CargoSaas
```

2. **Настройте строку подключения к БД**:

Отредактируйте `src/Cargo.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cargo_db;Username=ваш_пользователь;Password=ваш_пароль"
  }
}
```

3. **Восстановите зависимости**:
```bash
dotnet restore
```

4. **Создайте базу данных и примените миграции**:
```bash
cd src/Cargo.API
dotnet ef migrations add InitialCreate --project ../Cargo.Infrastructure --startup-project .
dotnet ef database update
```

5. **Запустите приложение**:
```bash
dotnet run --project src/Cargo.API
```

API будет доступен по адресу: `https://localhost:5001` (или `http://localhost:5000`)

Swagger UI: `https://localhost:5001/swagger`

## 📚 API Endpoints

### Tenants (Тенанты)

- `GET /api/tenants` - Получить всех тенантов
- `GET /api/tenants/{id}` - Получить тенанта по ID
- `GET /api/tenants/by-code/{tenantCode}` - Получить тенанта по коду
- `GET /api/tenants/active` - Получить активных тенантов
- `POST /api/tenants` - Создать нового тенанта
- `PUT /api/tenants/{id}` - Обновить тенанта
- `DELETE /api/tenants/{id}` - Удалить тенанта

### Tracks (Треки)

- `GET /api/tracks` - Получить все треки (текущего тенанта)
- `GET /api/tracks/{id}` - Получить трек по ID
- `GET /api/tracks/by-tracking-number/{trackingNumber}` - Получить трек по номеру
- `GET /api/tracks/by-client/{clientCode}` - Получить треки клиента
- `GET /api/tracks/by-status/{status}` - Получить треки по статусу
- `POST /api/tracks` - Создать новый трек
- `PUT /api/tracks/{id}` - Обновить трек
- `DELETE /api/tracks/{id}` - Удалить трек

## 🗄️ Структура базы данных

### Таблица: Tenants
```sql
- Id (uuid, PK)
- TenantId (uuid) - ссылка на самого себя
- CompanyName (varchar)
- TenantCode (varchar, unique)
- ContactEmail (varchar)
- ContactPhone (varchar, nullable)
- IsActive (boolean)
- SubscriptionExpiresAt (timestamp, nullable)
- CreatedAt (timestamp)
- UpdatedAt (timestamp, nullable)
```

### Таблица: Tracks
```sql
- Id (uuid, PK)
- TenantId (uuid, FK to Tenants)
- ClientCode (varchar)
- TrackingNumber (varchar, unique per tenant)
- Status (int)
- Description (text, nullable)
- Weight (decimal, nullable)
- DeclaredValue (decimal, nullable)
- OriginCountry (varchar, nullable)
- DestinationCountry (varchar, nullable)
- ShippedAt (timestamp, nullable)
- EstimatedDeliveryAt (timestamp, nullable)
- ActualDeliveryAt (timestamp, nullable)
- Notes (text, nullable)
- CreatedAt (timestamp)
- UpdatedAt (timestamp, nullable)
```

## 🔐 Безопасность (TODO)

В текущей версии аутентификация и авторизация **не реализованы**. 

Планируется добавить:
- JWT аутентификацию
- Role-based authorization
- Извлечение TenantId из JWT токена
- API Key для внешних интеграций

## 🧪 Тестирование

Для тестирования API используйте Swagger UI или любой HTTP-клиент (Postman, Insomnia).

### Пример создания тенанта:
```bash
curl -X POST https://localhost:5001/api/tenants \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Тестовая компания",
    "tenantCode": "test-company",
    "contactEmail": "test@company.com",
    "contactPhone": "+7 999 123-45-67"
  }'
```

### Пример создания трека:
```bash
curl -X POST https://localhost:5001/api/tracks \
  -H "Content-Type: application/json" \
  -d '{
    "clientCode": "CLIENT001",
    "trackingNumber": "TRACK123456",
    "description": "Электроника",
    "weight": 5.5,
    "originCountry": "Китай",
    "destinationCountry": "Россия"
  }'
```

## 📝 Entity Framework Core команды

### Создание новой миграции:
```bash
dotnet ef migrations add МиграцияИмя --project src/Cargo.Infrastructure --startup-project src/Cargo.API
```

### Применение миграций:
```bash
dotnet ef database update --project src/Cargo.API
```

### Откат миграции:
```bash
dotnet ef database update ПредыдущаяМиграция --project src/Cargo.API
```

### Удаление последней миграции:
```bash
dotnet ef migrations remove --project src/Cargo.Infrastructure --startup-project src/Cargo.API
```

## 🏗️ Дальнейшее развитие

- [ ] Добавить аутентификацию и авторизацию (JWT)
- [ ] Реализовать middleware для извлечения TenantId из HTTP-заголовка
- [ ] Добавить логирование (Serilog)
- [ ] Реализовать Unit-тесты
- [ ] Добавить пагинацию для списков
- [ ] Реализовать поиск и фильтрацию
- [ ] Добавить валидацию с FluentValidation
- [ ] Настроить CI/CD
- [ ] Добавить Docker-контейнеризацию
- [ ] Реализовать кэширование (Redis)
- [ ] Добавить event sourcing для истории изменений треков

## 📄 Лицензия

Этот проект создан для образовательных целей.

## 👥 Автор

Разработан как часть B2B SaaS решения для управления грузоперевозками.

