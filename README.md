# Cargo.Solution

**B2B SaaS Platform for International Cargo Tracking and Management**

---

## Overview

Cargo.Solution is a multi-tenant B2B SaaS platform designed for cargo logistics companies to manage and track international shipments. The platform provides real-time tracking, client management, and Excel-based bulk import capabilities through a Telegram WebApp interface.

---

## Key Features

- **Multi-tenant Architecture** - Secure data isolation between organizations
- **Telegram WebApp Integration** - Native mobile experience with built-in authentication
- **Real-time Tracking** - Track cargo status across international routes
- **Bulk Operations** - Excel import/export for efficient data management
- **Role-based Access Control** - SystemAdmin, Manager, and Client roles
- **RESTful API** - Clean architecture with Swagger documentation

---

## Technology Stack

### Backend
- **.NET 8** - Web API with Clean Architecture
- **ASP.NET Core Identity** - User authentication and authorization
- **Entity Framework Core 8** - ORM with Code-First approach
- **PostgreSQL 16** - Primary database
- **JWT** - Token-based authentication
- **EPPlus** - Excel file processing
- **Telegram.Bot** - Bot integration for notifications

### Frontend
- **React 18** - UI library
- **TypeScript** - Type-safe development
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Utility-first styling
- **@twa-dev/sdk** - Telegram WebApp SDK
- **Axios** - HTTP client

### Infrastructure
- **Docker** - Containerization
- **Railway** - Cloud hosting platform
- **GitHub Actions** - CI/CD (auto-deploy)
- **Nginx** - Static file serving (frontend)

---

## Architecture

```
┌─────────────────────────────────────────┐
│           Cargo.API (REST API)          │
│     Controllers, DTOs, Middleware       │
└───────────────┬─────────────────────────┘
                │
┌───────────────▼─────────────────────────┐
│      Cargo.Infrastructure               │
│   DbContext, Repositories, Services     │
└───────────────┬─────────────────────────┘
                │
┌───────────────▼─────────────────────────┐
│         Cargo.Core (Domain)             │
│    Entities, Interfaces, Business Logic │
└─────────────────────────────────────────┘
```

**Design Patterns:**
- Clean Architecture (Onion Architecture)
- Repository Pattern
- Unit of Work
- Dependency Injection
- CQRS principles

---

## Project Structure

```
CargoSaas/
├── src/
│   ├── Cargo.API/              # REST API Layer
│   │   ├── Controllers/        # API endpoints
│   │   ├── DTOs/               # Data Transfer Objects
│   │   └── Program.cs          # DI container & middleware
│   │
│   ├── Cargo.Infrastructure/   # Data Access Layer
│   │   ├── Data/               # DbContext, migrations
│   │   ├── Repositories/       # Data access implementations
│   │   └── Services/           # Business services
│   │
│   ├── Cargo.Core/             # Domain Layer
│   │   ├── Entities/           # Domain models
│   │   ├── Interfaces/         # Abstractions
│   │   └── Enums/              # Enumerations
│   │
│   └── Cargo.Web/              # Frontend (React + TS)
│       ├── src/
│       │   ├── components/     # React components
│       │   ├── contexts/       # Context providers
│       │   ├── pages/          # Page components
│       │   └── api/            # API client
│       └── Dockerfile
│
├── Dockerfile                  # Backend container
├── docker-compose.yml          # Local development
└── Cargo.Solution.sln          # .NET solution
```

---

## Database Schema

### Core Entities

**Tenants** - Organizations using the platform
- Multi-tenant isolation via `TenantId`
- Subscription management
- Company information

**Tracks** - Cargo shipments
- Tracking number, status, weight
- Origin/destination countries
- Estimated/actual delivery dates
- Client assignment

**AspNetUsers (AppUser)** - Users with Single Table Inheritance
- Managers - Email/password authentication
- Clients - Telegram authentication
- Role-based permissions

---

## Security Features

- **HMAC-SHA256 Validation** - Telegram initData verification
- **JWT Authentication** - Secure token-based auth
- **Global Query Filters** - Automatic tenant isolation
- **Password Hashing** - ASP.NET Core Identity (PBKDF2)
- **Role-based Authorization** - Granular access control

---

## API Endpoints

### Authentication
- `POST /api/client/auth` - Telegram WebApp authentication

### Tracks Management
- `GET /api/tracks` - List all tracks (tenant-filtered)
- `GET /api/tracks/{id}` - Get track details
- `POST /api/tracks` - Create new track
- `PUT /api/tracks/{id}` - Update track
- `DELETE /api/tracks/{id}` - Delete track

### Import/Export
- `POST /api/import/tracks` - Bulk import from Excel
- `GET /api/tracks/export` - Export to Excel

### System
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation

---

## Environment Variables

### Backend (Cargo.API)

```bash
DATABASE_URL=postgresql://user:password@host:5432/dbname
Jwt__SecretKey=your-secret-key-min-32-characters
Jwt__Issuer=CargoAPI
Jwt__Audience=CargoWebApp
Jwt__ExpirationMinutes=43200
Telegram__BotToken=your-bot-token-from-botfather
Telegram__WebAppUrl=https://your-frontend-url
```

### Frontend (Cargo.Web)

```bash
VITE_API_URL=https://your-backend-url/api
```

---

## Local Development

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** (optional, can use Docker)

### Quick Start

#### 1. Clone Repository

```bash
git clone https://github.com/your-username/CargoSaas.git
cd CargoSaas
```

#### 2. Backend Setup

```bash
# Restore dependencies
dotnet restore

# Apply migrations
cd src/Cargo.API
dotnet ef database update

# Run API
dotnet run
# API available at: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

#### 3. Frontend Setup

```bash
cd src/Cargo.Web

# Install dependencies
npm install

# Run dev server
npm run dev
# Frontend available at: http://localhost:5173
```

#### 4. Docker Compose (Full Stack)

```bash
# Create .env file
cp .env.example .env
# Edit .env with your configuration

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

---

## Deployment

### Railway (Production)

**Backend:**
1. Create new service from GitHub repository
2. Select `Dockerfile` as build source
3. Add environment variables (see above)
4. Deploy automatically on git push

**Frontend:**
1. Create new service from GitHub repository (`src/Cargo.Web/`)
2. Set build args: `VITE_API_URL`
3. Deploy automatically on git push

**PostgreSQL:**
1. Add PostgreSQL service
2. Railway automatically provides `DATABASE_URL`

---

## Multi-Tenancy Implementation

### Strategy: Discriminator Column

Each entity inherits from `BaseEntity`:

```csharp
public abstract class BaseEntity 
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }  // Tenant isolation
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Global Query Filter

Automatically applied to all queries:

```csharp
modelBuilder.Entity<Track>()
    .HasQueryFilter(t => t.TenantId == _currentTenantId);
```

Tenant ID is extracted from JWT token claims by `HttpContextTenantProvider`.

---

## Telegram Bot Integration

### Features
- `/start` command - Welcome message with WebApp button
- Long Polling - Real-time message handling
- WebApp Authentication - HMAC-SHA256 validation

### Bot Setup

1. Create bot via [@BotFather](https://t.me/BotFather)
2. Get Bot Token
3. Set WebApp URL: `/mybots` → `Bot Settings` → `Menu Button`
4. Add token to environment variables

---

## Performance Considerations

- **Database Indexing** - TenantId, TelegramId, TrackingNumber
- **Async/Await** - All I/O operations are asynchronous
- **Connection Pooling** - EF Core connection pooling enabled
- **Global Query Filters** - Automatic tenant filtering (prevents data leaks)

---

## Monitoring & Health Checks

### Health Check Endpoint

```bash
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-12-04T18:00:00Z"
}
```

### Logging

- **Console Logging** - Structured logs to stdout
- **EF Core Query Logging** - Sensitive data logging disabled in production
- **Exception Handling** - Global exception middleware

---

## Migration Management

### Create New Migration

```bash
cd src/Cargo.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../Cargo.API
```

### Apply Migrations

```bash
# Automatic on startup (Program.cs)
context.Database.Migrate();

# Or manually
dotnet ef database update --startup-project ../Cargo.API
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName --startup-project ../Cargo.API
```

---

## Testing

### Run Unit Tests (when available)

```bash
dotnet test
```

### Manual API Testing

Use Swagger UI:
```
http://localhost:8080/swagger
```

Or Postman collection (TBD).

---

## Troubleshooting

### Common Issues

**"Connection to database failed"**
- Check `DATABASE_URL` environment variable
- Ensure PostgreSQL is running
- Verify connection string format

**"Telegram Bot 401 Unauthorized"**
- Check `Telegram__BotToken` is correct
- Verify bot token from @BotFather

**"Global query filter not working"**
- Ensure user is authenticated (JWT token present)
- Check `tenantId` claim exists in token
- Verify `HttpContextTenantProvider` is registered

**"Migration failed"**
- Drop database and recreate (development only)
- Check PostgreSQL syntax (use `"ColumnName"`, not `[ColumnName]`)

---

## Database Backup

### Manual Backup

```bash
# Railway CLI
railway run pg_dump > backup_$(date +%F).sql

# Or use pgAdmin / DBeaver
```

### Restore from Backup

```bash
railway run psql < backup_2024-12-04.sql
```

---

## License

**Proprietary** - All rights reserved.

This software is confidential and proprietary information. Unauthorized copying, distribution, or use is strictly prohibited.

---

## Technical Support

For technical issues, deployment questions, or feature requests, contact the development team.

**Project Status:** ✅ Production Ready (MVP)

**Version:** 1.0.0  
**Last Updated:** December 2024

---

## Changelog

### v1.0.0 (2024-12-04)
- ✅ Initial release
- ✅ Multi-tenant architecture
- ✅ Telegram WebApp integration
- ✅ JWT authentication
- ✅ Excel import/export
- ✅ Railway deployment
- ✅ Docker containerization
- ✅ ASP.NET Core Identity integration

---

## Future Roadmap

- [ ] Webhook support for Telegram Bot (instead of Long Polling)
- [ ] JWT Refresh Tokens
- [ ] Unit Tests (controllers, services)
- [ ] Email notifications
- [ ] Advanced filtering & search
- [ ] QR code scanning
- [ ] Mobile apps (iOS/Android) via React Native
- [ ] Real-time updates via SignalR
- [ ] Analytics dashboard
- [ ] Multi-language support (i18n)

---

**Built with ❤️ for efficient cargo logistics management**
