# 🚂 Деплой на Railway.app

## Быстрый деплой на Railway

Railway - это платформа для автоматического деплоя, которая идеально подходит для .NET приложений с PostgreSQL.

### Предварительные требования

1. Аккаунт на [Railway.app](https://railway.app)
2. GitHub аккаунт (для подключения репозитория)
3. Проект должен быть в публичном или приватном GitHub репозитории

---

## Шаг 1: Подготовка проекта

### 1.1. Проверьте .gitignore

Убедитесь, что `.gitignore` исключает чувствительные данные:

```gitignore
# Environment
.env
.env.local
.env.production

# User-specific
appsettings.Development.json  # Если содержит реальные пароли
```

### 1.2. Создайте Railway конфигурацию

Файл уже создан: `railway.toml` (см. ниже)

### 1.3. Убедитесь, что миграции автоматические

В `Program.cs` уже есть код для автоматического применения миграций:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = services.GetRequiredService<CargoDbContext>();
    context.Database.Migrate(); // Автоматическое применение миграций
}
```

---

## Шаг 2: Настройка в Railway Dashboard

### 2.1. Создайте новый проект

1. Зайдите на [Railway.app](https://railway.app)
2. Нажмите **"New Project"**
3. Выберите **"Deploy from GitHub repo"**
4. Авторизуйте Railway для доступа к вашему GitHub
5. Выберите репозиторий **CargoSaas**

### 2.2. Добавьте PostgreSQL

1. В проекте нажмите **"+ New"**
2. Выберите **"Database"**
3. Выберите **"Add PostgreSQL"**
4. Railway автоматически создаст БД и предоставит переменную `DATABASE_URL`

### 2.3. Настройте переменные окружения

В разделе **Variables** вашего сервиса добавьте:

```bash
# Railway автоматически предоставит DATABASE_URL
# Но нужно преобразовать его в формат для .NET

# Если DATABASE_URL выглядит так:
# postgresql://user:pass@host:5432/dbname

# То создайте переменную:
ConnectionStrings__DefaultConnection=Host=your-db.railway.app;Port=5432;Database=railway;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true

# Или просто используйте DATABASE_URL напрямую (см. Program.cs ниже)
```

**Важно**: Railway предоставляет `DATABASE_URL` автоматически. Лучше использовать его напрямую.

### 2.4. Настройте остальные переменные

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

Railway автоматически предоставит переменную `$PORT`.

---

## Шаг 3: Обновите Program.cs для Railway

Добавьте поддержку `DATABASE_URL` от Railway:

```csharp
// В начале Program.cs, после builder.Services.AddControllers();

// Railway DATABASE_URL support
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Парсинг DATABASE_URL от Railway
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
    // Локальная разработка
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<CargoDbContext>(options =>
    options.UseNpgsql(connectionString));
```

---

## Шаг 4: Создайте railway.toml

```toml
[build]
builder = "NIXPACKS"
buildCommand = "dotnet restore && dotnet publish src/Cargo.API/Cargo.API.csproj -c Release -o /app/publish"

[deploy]
startCommand = "cd /app/publish && dotnet Cargo.API.dll"
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 10

[env]
ASPNETCORE_ENVIRONMENT = "Production"
```

Этот файл уже создан в корне проекта.

---

## Шаг 5: Деплой

1. **Push в GitHub**:
```bash
git add .
git commit -m "Prepare for Railway deployment"
git push origin main
```

2. **Railway автоматически задеплоит** ваше приложение

3. **Проверьте логи** в Railway Dashboard

4. **Получите URL** вашего приложения (Railway предоставит автоматически)

---

## Переменные окружения в Railway

### Обязательные

| Переменная | Значение | Описание |
|------------|----------|----------|
| `DATABASE_URL` | *автоматически* | Railway PostgreSQL connection |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Режим окружения |
| `PORT` | *автоматически* | Порт от Railway |

### Опциональные

| Переменная | Значение | Описание |
|------------|----------|----------|
| `ConnectionStrings__DefaultConnection` | *connection string* | Если не используете DATABASE_URL |

---

## Проверка деплоя

После деплоя проверьте:

```bash
# Healthcheck
curl https://your-app.railway.app/health

# Swagger (если включен в production)
https://your-app.railway.app/swagger

# API
curl https://your-app.railway.app/api/tenants
```

---

## Troubleshooting

### Проблема: Приложение не запускается

**Решение**: Проверьте логи в Railway Dashboard

```bash
# Типичные ошибки:
# 1. Connection string неверный
# 2. Миграции не применились
# 3. PORT не настроен
```

### Проблема: Не могу подключиться к БД

**Решение**: 
1. Проверьте, что PostgreSQL сервис запущен
2. Проверьте переменную `DATABASE_URL`
3. Убедитесь, что используете `SSL Mode=Require`

### Проблема: Миграции не применяются

**Решение**: 
- Проверьте логи при старте приложения
- Убедитесь, что код автоматических миграций в `Program.cs` присутствует

---

## Настройка Custom Domain (опционально)

1. В Railway Dashboard откройте ваш сервис
2. Перейдите в **Settings** → **Domains**
3. Нажмите **"Add Domain"**
4. Введите ваш домен: `api.yourdomain.com`
5. Настройте CNAME запись в вашем DNS:
   ```
   CNAME api -> your-app.railway.app
   ```
6. Railway автоматически выпустит SSL сертификат

---

## Масштабирование на Railway

Railway автоматически масштабирует ваше приложение по мере необходимости.

### Мониторинг ресурсов

В Railway Dashboard вы можете видеть:
- CPU usage
- Memory usage
- Network traffic
- Request logs

### Вертикальное масштабирование

Можно увеличить ресурсы в **Settings** → **Resources**

---

## Стоимость

Railway предоставляет:
- **$5 бесплатно** каждый месяц (для hobby проектов)
- **Pay-as-you-go** после использования бесплатных кредитов

Примерная стоимость для малого проекта: **$5-10/месяц**

---

## Альтернативы Railway

Если Railway не подходит, рассмотрите:

1. **Heroku** (аналогично Railway, но дороже)
2. **Render.com** (хорошая альтернатива)
3. **Fly.io** (больше контроля)
4. **Azure App Service** (для enterprise)
5. **AWS Elastic Beanstalk**
6. **Google Cloud Run**

---

## CI/CD с GitHub Actions (опционально)

Railway автоматически деплоит при push в GitHub, но можно настроить дополнительные проверки:

```yaml
# .github/workflows/railway-deploy.yml
name: Railway Deploy

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

## Резюме: Чек-лист для деплоя

- [ ] Убрали личные данные из кода
- [ ] Добавили `.gitignore` для `.env`
- [ ] Создали `railway.toml`
- [ ] Обновили `Program.cs` для поддержки `DATABASE_URL`
- [ ] Запушили код в GitHub
- [ ] Создали проект на Railway
- [ ] Добавили PostgreSQL в Railway
- [ ] Настроили переменные окружения
- [ ] Проверили деплой через healthcheck
- [ ] Проверили Swagger/API endpoints

---

**Удачного деплоя на Railway!** 🚂🚀


