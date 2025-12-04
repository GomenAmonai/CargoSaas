# Руководство по миграциям Entity Framework Core

## Первоначальная настройка базы данных

### 1. Установка инструментов EF Core

Убедитесь, что у вас установлен инструмент dotnet-ef:

```bash
dotnet tool install --global dotnet-ef
```

Или обновите до последней версии:

```bash
dotnet tool update --global dotnet-ef
```

### 2. Проверка подключения к PostgreSQL

Убедитесь, что PostgreSQL запущен и доступен. Создайте базу данных:

```sql
CREATE DATABASE cargo_db;
```

Или используйте команду из терминала:

```bash
createdb cargo_db
```

### 3. Настройка строки подключения

Отредактируйте файл `src/Cargo.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cargo_db;Username=ваш_юзер;Password=ваш_пароль"
  }
}
```

### 4. Создание первой миграции

Из корневой директории проекта выполните:

```bash
cd src/Cargo.API
dotnet ef migrations add InitialCreate --project ../Cargo.Infrastructure --startup-project .
```

Эта команда создаст папку `Migrations` в проекте `Cargo.Infrastructure` с начальной миграцией.

### 5. Применение миграции к базе данных

```bash
dotnet ef database update
```

Эта команда создаст таблицы `Tenants` и `Tracks` в базе данных PostgreSQL.

## Работа с миграциями

### Создание новой миграции

Когда вы изменяете модели данных, создайте новую миграцию:

```bash
dotnet ef migrations add ИмяВашейМиграции --project ../Cargo.Infrastructure --startup-project .
```

Примеры имён миграций:
- `AddIndexToTrackingNumber`
- `AddEmailToTenant`
- `RemoveUnusedColumns`

### Просмотр SQL-скрипта миграции

Чтобы увидеть SQL, который будет выполнен:

```bash
dotnet ef migrations script
```

Или для конкретной миграции:

```bash
dotnet ef migrations script ПредыдущаяМиграция НоваяМиграция
```

### Откат миграции

Откат к предыдущей миграции:

```bash
dotnet ef database update ИмяПредыдущейМиграции
```

Откат всех миграций (очистка БД):

```bash
dotnet ef database update 0
```

### Удаление последней миграции

Если миграция еще не применена к базе данных:

```bash
dotnet ef migrations remove --project ../Cargo.Infrastructure --startup-project .
```

### Список всех миграций

```bash
dotnet ef migrations list
```

## Наполнение базы данных тестовыми данными

### Вариант 1: Через SQL

Создайте файл `seed-data.sql`:

```sql
-- Создание тестового тенанта
INSERT INTO "Tenants" ("Id", "TenantId", "CompanyName", "TenantCode", "ContactEmail", "IsActive", "CreatedAt")
VALUES 
  ('a1111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111', 'Тестовая Компания 1', 'test-company-1', 'test1@example.com', true, NOW()),
  ('b2222222-2222-2222-2222-222222222222', 'b2222222-2222-2222-2222-222222222222', 'Тестовая Компания 2', 'test-company-2', 'test2@example.com', true, NOW());

-- Создание тестовых треков
INSERT INTO "Tracks" ("Id", "TenantId", "ClientCode", "TrackingNumber", "Status", "OriginCountry", "DestinationCountry", "CreatedAt")
VALUES 
  (gen_random_uuid(), 'a1111111-1111-1111-1111-111111111111', 'CLIENT001', 'TRACK001', 0, 'Китай', 'Россия', NOW()),
  (gen_random_uuid(), 'a1111111-1111-1111-1111-111111111111', 'CLIENT001', 'TRACK002', 1, 'Германия', 'Россия', NOW()),
  (gen_random_uuid(), 'b2222222-2222-2222-2222-222222222222', 'CLIENT002', 'TRACK003', 2, 'США', 'Канада', NOW());
```

Выполните:

```bash
psql -d cargo_db -f seed-data.sql
```

### Вариант 2: Через API

После запуска приложения используйте Swagger UI (`https://localhost:5001/swagger`) или curl:

```bash
# Создание тенанта
curl -X POST https://localhost:5001/api/tenants \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Моя Компания",
    "tenantCode": "my-company",
    "contactEmail": "info@mycompany.com"
  }'

# Создание трека (замените tenantId на полученный ID)
curl -X POST https://localhost:5001/api/tracks \
  -H "Content-Type: application/json" \
  -d '{
    "clientCode": "CLIENT001",
    "trackingNumber": "TRACK123456",
    "originCountry": "Китай",
    "destinationCountry": "Россия"
  }'
```

## Типичные проблемы и решения

### Ошибка: "Build failed"

Убедитесь, что проект компилируется:

```bash
dotnet build
```

### Ошибка: "No DbContext was found"

Убедитесь, что вы указали правильные проекты:
- `--project` - проект с DbContext (Infrastructure)
- `--startup-project` - проект с конфигурацией (API)

### Ошибка подключения к PostgreSQL

Проверьте:
1. Запущен ли PostgreSQL: `pg_isready`
2. Правильность строки подключения
3. Существует ли база данных
4. Доступны ли права пользователя

### Конфликт миграций

Если возникли конфликты:

1. Откатите базу данных: `dotnet ef database update 0`
2. Удалите папку Migrations
3. Создайте миграцию заново: `dotnet ef migrations add InitialCreate`
4. Примените: `dotnet ef database update`

## Полезные команды

```bash
# Получение информации о DbContext
dotnet ef dbcontext info

# Генерация SQL-скрипта для всех миграций
dotnet ef migrations script -o migrations.sql

# Применение конкретной миграции
dotnet ef database update МиграцияИмя

# Проверка статуса миграций
dotnet ef migrations list
```

## Лучшие практики

1. **Всегда проверяйте сгенерированный SQL** перед применением миграций к production
2. **Делайте резервные копии** базы данных перед применением миграций
3. **Используйте осмысленные имена** для миграций
4. **Не удаляйте старые миграции**, если они уже применены
5. **Тестируйте откат миграций** перед деплоем
6. **Коммитьте файлы миграций** в систему контроля версий

## Docker для PostgreSQL (опционально)

Если у вас нет установленного PostgreSQL:

```bash
docker run --name cargo-postgres \
  -e POSTGRES_DB=cargo_db \
  -e POSTGRES_USER=cargo_user \
  -e POSTGRES_PASSWORD=cargo_password \
  -p 5432:5432 \
  -d postgres:16
```

Строка подключения:
```
Host=localhost;Port=5432;Database=cargo_db;Username=cargo_user;Password=cargo_password
```

