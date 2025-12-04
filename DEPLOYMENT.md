# 🚀 Памятка по деплою Cargo.Solution

## Быстрый деплой с Docker (рекомендуется)

### Шаг 1: Подготовка сервера

```bash
# Установите Docker и Docker Compose на сервере
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Установите Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.23.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Проверка
docker --version
docker-compose --version
```

### Шаг 2: Клонирование проекта

```bash
# Клонируйте проект на сервер
git clone https://github.com/your-username/CargoSaas.git
cd CargoSaas
```

### Шаг 3: Настройка переменных окружения

```bash
# Создайте .env файл
cp env.example.txt .env

# Отредактируйте .env
nano .env
```

**Важно**: Установите надёжные пароли для production!

```env
POSTGRES_PASSWORD=YourVerySecurePassword123!
POSTGRES_USER=cargo_user
POSTGRES_DB=cargo_db

PGADMIN_EMAIL=admin@yourcompany.com
PGADMIN_PASSWORD=AnotherSecurePassword456!

ASPNETCORE_ENVIRONMENT=Production
```

### Шаг 4: Запуск

```bash
# Соберите и запустите все сервисы
docker-compose up -d --build

# Проверьте статус
docker-compose ps

# Проверьте логи
docker-compose logs -f cargo-api
```

### Шаг 5: Проверка работоспособности

```bash
# Healthcheck
curl http://localhost:8080/health

# Swagger (если нужен)
curl http://localhost:8080/swagger/index.html
```

### Шаг 6: Настройка Nginx (опционально)

```nginx
# /etc/nginx/sites-available/cargo-api
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Активируйте конфигурацию
sudo ln -s /etc/nginx/sites-available/cargo-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Шаг 7: SSL с Let's Encrypt (опционально)

```bash
# Установите certbot
sudo apt install certbot python3-certbot-nginx

# Получите SSL сертификат
sudo certbot --nginx -d api.yourdomain.com

# Автоматическое обновление
sudo certbot renew --dry-run
```

---

## Деплой без Docker (альтернатива)

### Шаг 1: Установка .NET 8

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

### Шаг 2: Установка PostgreSQL

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install postgresql postgresql-contrib

# Создайте БД и пользователя
sudo -u postgres psql
CREATE DATABASE cargo_db;
CREATE USER cargo_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE cargo_db TO cargo_user;
\q
```

### Шаг 3: Сборка проекта

```bash
cd CargoSaas
dotnet restore
dotnet publish src/Cargo.API/Cargo.API.csproj -c Release -o /var/www/cargo-api
```

### Шаг 4: Настройка appsettings

```bash
# Отредактируйте production настройки
nano /var/www/cargo-api/appsettings.json
```

### Шаг 5: Systemd Service

```bash
# Создайте systemd service
sudo nano /etc/systemd/system/cargo-api.service
```

```ini
[Unit]
Description=Cargo API Service
After=network.target

[Service]
WorkingDirectory=/var/www/cargo-api
ExecStart=/usr/local/bin/dotnet /var/www/cargo-api/Cargo.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=cargo-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Запустите сервис
sudo systemctl daemon-reload
sudo systemctl enable cargo-api
sudo systemctl start cargo-api
sudo systemctl status cargo-api
```

---

## Мониторинг и обслуживание

### Просмотр логов

```bash
# Docker
docker-compose logs -f cargo-api

# Systemd
sudo journalctl -u cargo-api -f
```

### Обновление приложения

```bash
# Docker
git pull
docker-compose up -d --build

# Без Docker
git pull
dotnet publish src/Cargo.API/Cargo.API.csproj -c Release -o /var/www/cargo-api
sudo systemctl restart cargo-api
```

### Бэкап базы данных

```bash
# Docker
docker exec cargo-postgres pg_dump -U cargo_user cargo_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Без Docker
pg_dump -U cargo_user -d cargo_db -f backup_$(date +%Y%m%d_%H%M%S).sql
```

### Восстановление базы данных

```bash
# Docker
docker exec -i cargo-postgres psql -U cargo_user cargo_db < backup_20251204_120000.sql

# Без Docker
psql -U cargo_user -d cargo_db -f backup_20251204_120000.sql
```

---

## Безопасность в Production

### Checklist

- [ ] Используйте сильные пароли для БД
- [ ] Включите SSL/TLS (HTTPS)
- [ ] Настройте firewall (UFW/iptables)
- [ ] Ограничьте доступ к PostgreSQL (только localhost или VPC)
- [ ] Регулярно обновляйте зависимости
- [ ] Настройте автоматические бэкапы
- [ ] Используйте Docker secrets вместо .env в production
- [ ] Отключите Swagger в production (или защитите паролем)
- [ ] Настройте rate limiting
- [ ] Включите логирование и мониторинг

### Firewall (UFW)

```bash
# Разрешите только необходимые порты
sudo ufw allow 22/tcp      # SSH
sudo ufw allow 80/tcp      # HTTP
sudo ufw allow 443/tcp     # HTTPS
sudo ufw enable
sudo ufw status
```

### PostgreSQL Security

```bash
# Запретите внешние подключения (если API на том же сервере)
sudo nano /etc/postgresql/16/main/postgresql.conf
# listen_addresses = 'localhost'

sudo nano /etc/postgresql/16/main/pg_hba.conf
# host    cargo_db    cargo_user    127.0.0.1/32    scram-sha-256

sudo systemctl restart postgresql
```

---

## Производительность

### Настройка PostgreSQL

```bash
sudo nano /etc/postgresql/16/main/postgresql.conf
```

```ini
# Для сервера с 4GB RAM
shared_buffers = 1GB
effective_cache_size = 3GB
maintenance_work_mem = 256MB
checkpoint_completion_target = 0.9
wal_buffers = 16MB
default_statistics_target = 100
random_page_cost = 1.1
effective_io_concurrency = 200
work_mem = 10MB
min_wal_size = 1GB
max_wal_size = 4GB
max_worker_processes = 4
max_parallel_workers_per_gather = 2
max_parallel_workers = 4
max_parallel_maintenance_workers = 2
```

### Индексы

Индексы уже настроены в EF Core миграциях:
- `Tenants.TenantCode` (UNIQUE)
- `Tracks.TrackingNumber`
- `Tracks.TenantId + TrackingNumber` (UNIQUE COMPOSITE)

---

## Troubleshooting

### Проблема: API не запускается

```bash
# Проверьте логи
docker-compose logs cargo-api

# Проверьте connection string
docker exec -it cargo-api cat appsettings.json
```

### Проблема: Не могу подключиться к БД

```bash
# Проверьте, что PostgreSQL запущен
docker-compose ps postgres

# Проверьте подключение
docker exec -it cargo-postgres psql -U cargo_user -d cargo_db
```

### Проблема: Миграции не применяются

```bash
# Проверьте логи при старте
docker-compose logs cargo-api | grep -i migration

# Примените миграции вручную
docker exec -it cargo-api bash
dotnet ef database update
```

---

## Контакты для поддержки

- **Email**: support@cargo.example.com
- **Документация**: См. README.md, DOCKER_GUIDE.md
- **Логи**: `docker-compose logs -f` или `journalctl -u cargo-api -f`

---

**Удачного деплоя!** 🚀

