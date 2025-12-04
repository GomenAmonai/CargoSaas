# 🐳 Docker Deployment Guide для Cargo.Solution

## Содержание

1. [Быстрый старт](#быстрый-старт)
2. [Файлы Docker](#файлы-docker)
3. [Production деплой](#production-деплой)
4. [Development окружение](#development-окружение)
5. [Команды Docker](#команды-docker)
6. [Troubleshooting](#troubleshooting)

---

## Быстрый старт

### Предварительные требования

- Docker Engine 20.10+
- Docker Compose 2.0+

Проверка версий:
```bash
docker --version
docker-compose --version
```

### Запуск в Production режиме

```bash
# 1. Клонируйте/перейдите в папку проекта
cd /path/to/CargoSaas

# 2. Создайте .env файл из примера
cp .env.example .env

# 3. Отредактируйте .env и установите пароли
nano .env

# 4. Запустите весь стек (API + PostgreSQL)
docker-compose up -d

# 5. Проверьте логи
docker-compose logs -f cargo-api

# 6. Откройте в браузере
# API: http://localhost:8080/swagger
```

### Запуск в Development режиме

```bash
# Запуск dev окружения с hot reload
docker-compose -f docker-compose.dev.yml up -d

# API будет доступен на http://localhost:5000
```

---

## Файлы Docker

### 1. `Dockerfile` (Production)

Multi-stage build для оптимизации размера образа:

- **Stage 1 (build)**: Сборка проекта на `dotnet/sdk:8.0`
- **Stage 2 (publish)**: Публикация в Release режиме
- **Stage 3 (final)**: Финальный образ на `dotnet/aspnet:8.0` (меньше размер)

Особенности:
- ✅ Непривилегированный пользователь (appuser)
- ✅ Healthcheck для мониторинга
- ✅ Оптимизация кэширования слоёв
- ✅ Минимальный размер образа (~200MB)

### 2. `Dockerfile.dev` (Development)

Образ для разработки с:
- Hot reload (`dotnet watch`)
- dotnet-ef инструменты
- Debug логирование

### 3. `docker-compose.yml` (Production)

Полный стек для production:
- **cargo-api**: Ваше ASP.NET Core приложение
- **postgres**: PostgreSQL 16
- **pgadmin**: Web UI для управления БД (опционально)

### 4. `docker-compose.dev.yml` (Development)

Стек для разработки с volume mappings и debug настройками.

### 5. `.dockerignore`

Исключает ненужные файлы из Docker образа (bin, obj, .git, и т.д.)

### 6. `.env.example`

Шаблон переменных окружения.

---

## Production деплой

### Шаг 1: Подготовка

```bash
# Создайте .env файл
cp .env.example .env

# Отредактируйте .env
nano .env
```

Пример `.env`:
```env
POSTGRES_PASSWORD=YourSecurePassword123!
POSTGRES_USER=cargo_user
POSTGRES_DB=cargo_db

PGADMIN_EMAIL=admin@yourcompany.com
PGADMIN_PASSWORD=AdminPassword123!

ASPNETCORE_ENVIRONMENT=Production
```

### Шаг 2: Сборка образов

```bash
# Собрать образ API
docker-compose build cargo-api

# Или собрать все сервисы
docker-compose build
```

### Шаг 3: Запуск

```bash
# Запустить все сервисы в фоне
docker-compose up -d

# Запустить только API и PostgreSQL (без pgAdmin)
docker-compose up -d postgres cargo-api
```

### Шаг 4: Проверка

```bash
# Проверить статус контейнеров
docker-compose ps

# Посмотреть логи API
docker-compose logs -f cargo-api

# Посмотреть логи PostgreSQL
docker-compose logs -f postgres

# Healthcheck
curl http://localhost:8080/health
```

### Шаг 5: Применение миграций

Миграции применяются **автоматически** при запуске API благодаря коду в `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = services.GetRequiredService<CargoDbContext>();
    context.Database.Migrate(); // Автоматическое применение миграций
}
```

Если нужно применить миграции вручную:

```bash
# Войти в контейнер API
docker exec -it cargo-api bash

# Применить миграции
dotnet ef database update
```

### Шаг 6: Доступ к приложению

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **pgAdmin**: http://localhost:5050 (если запустили с профилем `tools`)

```bash
# Запустить с pgAdmin
docker-compose --profile tools up -d
```

---

## Development окружение

### Запуск dev стека

```bash
# Запустить dev окружение
docker-compose -f docker-compose.dev.yml up -d

# Посмотреть логи с hot reload
docker-compose -f docker-compose.dev.yml logs -f cargo-api-dev
```

### Доступы в dev режиме

- **API**: http://localhost:5000
- **PostgreSQL**: localhost:5433 (другой порт!)
- **pgAdmin**: http://localhost:5051

### Hot Reload

Изменения в коде автоматически подхватываются благодаря `dotnet watch`.

### Подключение к dev БД из IDE

```
Host: localhost
Port: 5433
Database: cargo_db_dev
Username: cargo_dev
Password: dev_password
```

---

## Команды Docker

### Управление контейнерами

```bash
# Запустить
docker-compose up -d

# Остановить
docker-compose down

# Остановить с удалением volumes (ВНИМАНИЕ: удалит данные!)
docker-compose down -v

# Перезапустить конкретный сервис
docker-compose restart cargo-api

# Остановить конкретный сервис
docker-compose stop cargo-api

# Запустить конкретный сервис
docker-compose start cargo-api
```

### Логи

```bash
# Все логи
docker-compose logs

# Логи конкретного сервиса
docker-compose logs cargo-api

# Последние 100 строк
docker-compose logs --tail=100 cargo-api

# Follow режим (live)
docker-compose logs -f cargo-api
```

### Exec команды

```bash
# Войти в контейнер API
docker exec -it cargo-api bash

# Выполнить команду в контейнере
docker exec -it cargo-api dotnet --info

# Войти в PostgreSQL
docker exec -it cargo-postgres psql -U cargo_user -d cargo_db
```

### Очистка

```bash
# Удалить остановленные контейнеры
docker-compose rm

# Очистить неиспользуемые образы
docker image prune -a

# Очистить всё (контейнеры, сети, volumes)
docker system prune -a --volumes

# Пересобрать без кэша
docker-compose build --no-cache
```

### Информация

```bash
# Список запущенных контейнеров
docker-compose ps

# Статистика использования ресурсов
docker stats

# Информация о контейнере
docker inspect cargo-api

# Список volumes
docker volume ls

# Список сетей
docker network ls
```

---

## Мониторинг и Healthchecks

### Healthcheck endpoints

API имеет встроенный healthcheck:

```bash
# Проверка здоровья API
curl http://localhost:8080/health

# Через Docker
docker inspect --format='{{json .State.Health}}' cargo-api | jq
```

### Healthcheck для PostgreSQL

```bash
# Проверка подключения к БД
docker exec cargo-postgres pg_isready -U cargo_user -d cargo_db
```

### Мониторинг логов

```bash
# Реальное время
docker-compose logs -f --tail=50 cargo-api

# Фильтр по уровню логов
docker-compose logs cargo-api | grep ERROR
```

---

## Production Best Practices

### 1. Безопасность

```bash
# Используйте Docker secrets вместо .env для production
docker secret create postgres_password ./postgres_password.txt

# Не используйте root пользователя (уже настроено в Dockerfile)
# Проверка:
docker exec cargo-api whoami  # должно вывести: appuser
```

### 2. Обновление образов

```bash
# 1. Собрать новый образ
docker-compose build cargo-api

# 2. Остановить старый контейнер
docker-compose stop cargo-api

# 3. Удалить старый контейнер
docker-compose rm -f cargo-api

# 4. Запустить новый
docker-compose up -d cargo-api

# Или одной командой:
docker-compose up -d --build cargo-api
```

### 3. Бэкапы

```bash
# Бэкап PostgreSQL
docker exec cargo-postgres pg_dump -U cargo_user cargo_db > backup_$(date +%Y%m%d).sql

# Восстановление
docker exec -i cargo-postgres psql -U cargo_user cargo_db < backup_20251204.sql

# Бэкап volume
docker run --rm -v cargo_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres-backup.tar.gz /data
```

### 4. Логирование

Рекомендуется использовать centralized logging (ELK, Loki, и т.д.):

```yaml
# Добавить в docker-compose.yml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

---

## Troubleshooting

### Проблема: Контейнер не запускается

```bash
# Проверить логи
docker-compose logs cargo-api

# Проверить статус
docker-compose ps

# Проверить конфигурацию
docker-compose config
```

### Проблема: Не могу подключиться к БД

```bash
# Проверить, что PostgreSQL запущен
docker-compose ps postgres

# Проверить healthcheck
docker inspect cargo-postgres | grep -A 10 Health

# Проверить логи PostgreSQL
docker-compose logs postgres

# Проверить сетевое подключение
docker exec cargo-api ping postgres
```

### Проблема: Миграции не применяются

```bash
# Проверить логи API при старте
docker-compose logs cargo-api | grep -i migration

# Применить миграции вручную
docker exec -it cargo-api bash
cd /app
dotnet ef database update
```

### Проблема: Port уже используется

```bash
# Найти процесс, использующий порт
lsof -i :8080

# Изменить порт в docker-compose.yml
ports:
  - "8081:8080"  # Внешний порт 8081
```

### Проблема: Медленная сборка

```bash
# Очистить кэш Docker
docker builder prune

# Использовать BuildKit
DOCKER_BUILDKIT=1 docker-compose build
```

### Проблема: Нехватка места на диске

```bash
# Проверить размер образов
docker system df

# Очистить неиспользуемые данные
docker system prune -a --volumes

# Удалить конкретный volume
docker volume rm cargo_postgres_data
```

---

## CI/CD Integration

### GitHub Actions пример

```yaml
# .github/workflows/deploy.yml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker image
        run: docker build -t cargo-api:${{ github.sha }} .
      
      - name: Push to registry
        run: |
          echo ${{ secrets.DOCKER_PASSWORD }} | docker login -u ${{ secrets.DOCKER_USERNAME }} --password-stdin
          docker push cargo-api:${{ github.sha }}
```

---

## Дополнительные ресурсы

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Best practices for writing Dockerfiles](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [.NET on Docker](https://learn.microsoft.com/dotnet/core/docker/introduction)

---

**Версия**: 1.0  
**Дата**: Декабрь 2025  
**Автор**: Senior .NET Architect

