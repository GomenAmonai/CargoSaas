# 🎯 Manager Feature - Детальный план разработки (LEAN MVP)

> Проектирование и реализация функционала для менеджеров логистической компании
> 
> **⚠️ ВАЖНО: Это LEAN MVP подход. Без избыточных библиотек и сложностей.**
> **Цель: Запустить за неделю, а не строить на века.**

---

## 📋 Содержание

1. [Обзор функционала](#обзор-функционала)
2. [Архитектура](#архитектура)
3. [Backend API](#backend-api)
4. [Frontend UI/UX](#frontend-uiux)
5. [Аутентификация](#аутентификация)
6. [Этапы разработки](#этапы-разработки)
7. [Технические детали](#технические-детали)

---

## 🎯 Обзор функционала

### **Различия между Client и Manager**

| Функционал | Client (Telegram) | Manager (Web Admin) |
|-----------|-------------------|---------------------|
| **Доступ** | Только свои треки (по ClientCode) | Все треки тенанта |
| **Операции** | Просмотр (Read-only) | CRUD + Import/Export |
| **Аутентификация** | Telegram WebApp (HMAC) | **Telegram Login Widget** (тоже Telegram!) |
| **UI** | Telegram WebApp (mobile) | Полноценный Web Admin |
| **Роль** | `UserRole.Client` | `UserRole.Manager` |

### **Основные фичи для Manager**

#### ✅ **1. Управление треками**
- Просмотр всех треков компании (tenant)
- Создание новых треков
- Редактирование существующих треков
- Удаление треков
- Фильтрация и поиск

#### ✅ **2. Импорт/Экспорт**
- Массовый импорт треков из Excel
- Экспорт треков в Excel
- Шаблон для импорта
- Валидация данных при импорте

#### ✅ **3. Управление клиентами**
- Просмотр списка клиентов
- Поиск клиентов по ClientCode
- Просмотр треков конкретного клиента
- (Опционально) Создание ClientCode вручную

#### ✅ **4. Аналитика и статистика**
- Количество треков по статусам
- Количество треков по клиентам
- Треки в пути / доставленные
- График активности (опционально)

---

## 🏗️ Архитектура

### **1. Точки входа (Entry Points)**

```
┌─────────────────────────────────────────────┐
│         Пользователи системы                │
└─────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
┌──────────────┐       ┌──────────────┐
│   Client     │       │   Manager    │
│  (Telegram)  │       │  (Web Admin) │
└──────────────┘       └──────────────┘
        │                       │
        ▼                       ▼
┌──────────────┐       ┌──────────────┐
│  /client/*   │       │  /manager/*  │
│   API        │       │    API       │
└──────────────┘       └──────────────┘
        │                       │
        └───────────┬───────────┘
                    ▼
         ┌──────────────────┐
         │  CargoDbContext  │
         │  (Multi-tenant)  │
         └──────────────────┘
```

### **2. Маршруты (Routes)**

#### **Backend API**

```
/api/client/*           - Для Telegram клиентов (существует)
  ├─ POST   /client/auth
  ├─ GET    /client/tracks
  └─ GET    /client/tracks/{id}

/api/manager/*          - Для веб-менеджеров (НОВОЕ)
  ├─ POST   /manager/auth/login
  ├─ POST   /manager/auth/register
  ├─ GET    /manager/tracks
  ├─ GET    /manager/tracks/{id}
  ├─ POST   /manager/tracks
  ├─ PUT    /manager/tracks/{id}
  ├─ DELETE /manager/tracks/{id}
  ├─ POST   /manager/tracks/import
  ├─ GET    /manager/tracks/export
  ├─ GET    /manager/clients
  └─ GET    /manager/statistics
```

#### **Frontend Routes**

```
/                       - Landing page или редирект
/login                  - Логин для менеджеров
/manager/*              - Веб-админка для менеджеров (НОВОЕ)
  ├─ /manager/dashboard      - Главная страница с статистикой
  ├─ /manager/tracks         - Список всех треков
  ├─ /manager/tracks/new     - Создание нового трека
  ├─ /manager/tracks/{id}    - Детали трека
  ├─ /manager/tracks/{id}/edit - Редактирование трека
  ├─ /manager/clients        - Список клиентов
  └─ /manager/import         - Импорт/экспорт

/telegram/*             - Telegram WebApp (существует)
  ├─ /telegram/home
  ├─ /telegram/tracks
  └─ /telegram/tracks/{id}
```

---

## 🔧 Backend API

### **1. ManagerAuthController (УПРОЩЕННЫЙ!)**

```csharp
[ApiController]
[Route("api/manager/auth")]
public class ManagerAuthController : ControllerBase
{
    [HttpPost("telegram")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> AuthenticateWithTelegram(
        [FromBody] TelegramAuthRequestDto request)
    {
        // 1. Валидируем Telegram данные (HMAC-SHA256)
        //    Используем ТОТ ЖЕ ITelegramAuthService!
        
        // 2. Ищем пользователя по TelegramId (.IgnoreQueryFilters())
        
        // 3. Проверяем роль:
        //    - Если Role = Manager → OK
        //    - Если Role = Client → 403 Forbidden
        //    - Если не найден → 401 Unauthorized
        
        // 4. Генерируем JWT токен
        
        // 5. Возвращаем токен + user info
    }
}
```

**❌ УДАЛЕНО:**
- `POST /register` — менеджеров создает админ, не они сами
- Email/Password логика — используем только Telegram

### **2. ManagerTracksController**

```csharp
[ApiController]
[Route("api/manager/tracks")]
[Authorize(Roles = "Manager,SystemAdmin")]
public class ManagerTracksController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TrackDto>>> GetTracks(
        [FromQuery] TrackFilterDto filter)
    {
        // Фильтрация: status, clientCode, dateRange, search
        // Пагинация: page, pageSize
        // Сортировка: orderBy, orderDirection
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDto>> GetTrackById(Guid id)
    {
        // Получить трек по ID (с проверкой TenantId через Query Filter)
    }

    [HttpPost]
    public async Task<ActionResult<TrackDto>> CreateTrack(
        [FromBody] CreateTrackDto request)
    {
        // Создать новый трек
        // Валидация: TrackingNumber уникален в рамках тенанта
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TrackDto>> UpdateTrack(
        Guid id, 
        [FromBody] UpdateTrackDto request)
    {
        // Обновить существующий трек
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTrack(Guid id)
    {
        // Удалить трек (soft delete или hard delete?)
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportResultDto>> ImportFromExcel(
        IFormFile file)
    {
        // Использовать существующий ExcelImportService
    }

    [HttpGet("export")]
    public async Task<FileResult> ExportToExcel(
        [FromQuery] TrackFilterDto filter)
    {
        // Экспортировать треки в Excel
        // Применить фильтры если указаны
    }
}
```

### **3. ManagerClientsController**

```csharp
[ApiController]
[Route("api/manager/clients")]
[Authorize(Roles = "Manager,SystemAdmin")]
public class ManagerClientsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        // Получить всех клиентов тенанта (Role = Client)
        // С количеством треков у каждого
    }

    [HttpGet("{clientCode}")]
    public async Task<ActionResult<ClientDetailsDto>> GetClientByCode(
        string clientCode)
    {
        // Детальная информация о клиенте
        // Список всех его треков
    }
}
```

### **4. ManagerStatisticsController**

```csharp
[ApiController]
[Route("api/manager/statistics")]
[Authorize(Roles = "Manager,SystemAdmin")]
public class ManagerStatisticsController : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatisticsDto>> GetDashboard()
    {
        return new DashboardStatisticsDto
        {
            TotalTracks = ...,
            TracksByStatus = ...,
            ActiveClients = ...,
            TracksCreatedToday = ...,
            TracksInTransit = ...,
            TracksDeliveredThisWeek = ...
        };
    }
}
```

---

## 🎨 Frontend UI/UX

### **1. Технологический стек (МИНИМАЛИЗМ!)**

```typescript
// Используем ТОЛЬКО существующие
✅ React 19 + TypeScript
✅ Vite
✅ Tailwind CSS
✅ React Router DOM
✅ Axios

// НЕ ДОБАВЛЯЕМ (пока не прижмет):
❌ React Query — для 50 треков достаточно useState + useEffect
❌ React Hook Form — для 5 полей достаточно controlled inputs
❌ Zod — простые if (value === '') валидации
❌ TanStack Table — для списка достаточно array.map()
❌ date-fns — встроенный Date.toLocaleDateString()

// Добавим ТОЛЬКО если реально нужно:
⚠️ react-hot-toast — опционально, для красивых уведомлений
```

**Философия:** Вводи сложные инструменты, когда данные станут сложными. 
Сейчас решаешь задачу скорости, а не архитектурной красоты.

### **2. Компоненты (Component Structure)**

```
src/
├── pages/
│   ├── manager/                    # НОВЫЕ страницы
│   │   ├── Dashboard.tsx           # Главная с статистикой
│   │   ├── TrackList.tsx           # Список всех треков
│   │   ├── TrackDetails.tsx        # Детали трека
│   │   ├── TrackForm.tsx           # Создание/редактирование
│   │   ├── ClientList.tsx          # Список клиентов
│   │   ├── ClientDetails.tsx       # Детали клиента
│   │   └── ImportExport.tsx        # Импорт/экспорт
│   ├── auth/
│   │   └── ManagerLogin.tsx        # Логин для менеджеров
│   └── telegram/                   # Существующие
│       ├── Home.tsx
│       └── ...
│
├── components/
│   ├── manager/                    # НОВЫЕ компоненты
│   │   ├── Layout/
│   │   │   ├── ManagerLayout.tsx   # Layout с навигацией
│   │   │   ├── Sidebar.tsx         # Боковое меню
│   │   │   └── Header.tsx          # Шапка с user info
│   │   ├── TrackTable.tsx          # Таблица треков
│   │   ├── TrackFilters.tsx        # Фильтры для треков
│   │   ├── StatisticsCard.tsx      # Карточки статистики
│   │   └── ClientCard.tsx          # Карточка клиента
│   └── shared/                     # Общие
│       ├── Button.tsx
│       ├── Input.tsx
│       ├── Select.tsx
│       └── Modal.tsx
│
├── contexts/
│   ├── ManagerAuthContext.tsx      # НОВЫЙ контекст
│   └── ...
│
└── api/
    ├── manager.ts                  # НОВЫЙ API клиент
    └── client.ts                   # Существующий
```

### **3. Дизайн страниц**

#### **Dashboard (Главная)**

```
┌────────────────────────────────────────────────┐
│  Sidebar  │  Header (User + Logout)            │
├───────────┼────────────────────────────────────┤
│           │  📊 Dashboard                       │
│ 📊 Dash   │                                     │
│ 📦 Tracks │  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐  │
│ 👥 Clients│  │ 150 │ │  45 │ │ 120 │ │  89 │  │
│ 📤 Import │  │Total│ │Trans│ │Deliv│ │Clien│  │
│           │  └─────┘ └─────┘ └─────┘ └─────┘  │
│           │                                     │
│           │  📈 Tracks by Status                │
│           │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│           │                                     │
│           │  📋 Recent Tracks                   │
│           │  ┌──────────────────────────────┐  │
│           │  │ Track 1  │ Status │ Client  │  │
│           │  │ Track 2  │ Status │ Client  │  │
│           │  └──────────────────────────────┘  │
└───────────┴────────────────────────────────────┘
```

#### **Track List (Список треков)**

```
┌────────────────────────────────────────────────┐
│  Sidebar  │  📦 Tracks                          │
├───────────┼────────────────────────────────────┤
│           │  [Search] [Filter▼] [+ New Track]  │
│           │                                     │
│           │  ┌─────────────────────────────────┐│
│           │  │ #  │ Tracking │ Client │ Status ││
│           │  ├────┼──────────┼────────┼────────┤│
│           │  │ 1  │ TR-001   │ CLT-01 │ ✓ Deliv││
│           │  │ 2  │ TR-002   │ CLT-02 │ → Trans││
│           │  │ 3  │ TR-003   │ CLT-01 │ ⏱ Creat││
│           │  └─────────────────────────────────┘│
│           │                                     │
│           │  Pagination: < 1 2 3 >              │
└───────────┴────────────────────────────────────┘
```

#### **Track Form (Создание/редактирование)**

```
┌────────────────────────────────────────────────┐
│  Sidebar  │  ✏️ Edit Track: TR-001              │
├───────────┼────────────────────────────────────┤
│           │  Tracking Number: [TR-001     ]    │
│           │  Client Code:     [CLT-01   ▼]    │
│           │  Status:          [In Transit▼]    │
│           │  Description:     [____________]    │
│           │  Weight (kg):     [2.5        ]    │
│           │  Origin:          [China    ▼]    │
│           │  Destination:     [Russia   ▼]    │
│           │  Shipped Date:    [📅 2024-12-01]  │
│           │  Est. Delivery:   [📅 2024-12-15]  │
│           │                                     │
│           │  [Cancel] [Save Changes]            │
└───────────┴────────────────────────────────────┘
```

---

## 🔐 Аутентификация

### **⚡ КЛЮЧЕВОЕ РЕШЕНИЕ: Telegram для всех!**

**Почему НЕ email/password:**
- ❌ Новый вектор атаки (хранение паролей)
- ❌ Куча работы (восстановление, подтверждение email)
- ❌ Менеджеры = те же люди из Telegram
- ❌ B2B SaaS = закрытый клуб, не нужна публичная регистрация

**Почему Telegram Login Widget:**
- ✅ Та же безопасность HMAC-SHA256
- ✅ Нет паролей = нет утечек
- ✅ QR-код на десктопе = мгновенный вход
- ✅ Управление доступом через бота (`/promote @username`)
- ✅ Уже реализован механизм валидации

### **1. Manager Login Flow (Telegram Widget)**

```
1. Пользователь открывает admin.cargosaas.com
   ↓
2. Видит кнопку "Log in with Telegram" (виджет)
   ↓
3. Сканирует QR или вводит телефон → Telegram авторизация
   ↓
4. Telegram отправляет данные (подпись HMAC-SHA256)
   ↓
5. POST /api/manager/auth (тот же механизм валидации)
   ↓
6. Backend проверяет: TelegramId + Role = Manager?
   ↓
7. Если Manager → генерируем JWT
   ↓
8. Если Client → 403 Forbidden
   ↓
9. Редирект на /manager/dashboard
```

**Регистрация Manager = ОТСУТСТВУЕТ!**
- Менеджеров создает SuperAdmin через команду бота `/promote @username`
- Или через прямую вставку в БД (для первого запуска)

### **2. DTOs для аутентификации**

```csharp
// Telegram Auth (ОБЩИЙ для Client и Manager!)
public class TelegramAuthRequestDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PhotoUrl { get; set; }
    public long AuthDate { get; set; }
    public string Hash { get; set; } = string.Empty;
}

// Response (общий для Client и Manager)
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string Role { get; set; } = string.Empty; // "Client" или "Manager"
    public bool IsNewUser { get; set; }
}
```

**❌ УДАЛЕНЫ:**
- `LoginRequestDto` (email/password)
- `RegisterRequestDto` (публичная регистрация)

### **3. Protected Routes**

```typescript
// src/components/manager/ManagerRoute.tsx

export const ManagerRoute = ({ children }: { children: ReactNode }) => {
  const { user, isAuthenticated } = useManagerAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
    } else if (user?.role !== 'Manager' && user?.role !== 'SystemAdmin') {
      navigate('/403'); // Forbidden
    }
  }, [isAuthenticated, user, navigate]);

  if (!isAuthenticated) {
    return <div>Loading...</div>;
  }

  return <>{children}</>;
};
```

---

## 🚀 Этапы разработки

### **Phase 1: Рефакторинг (0.5-1 день) - МИНИМУМ**

1. ✅ Извлечь константы в `Constants.cs` (TenantId, ClientCode prefix)
2. ✅ Настроить CORS для конкретных доменов
3. ✅ Простые custom exceptions (без Result Pattern)
4. ❌ ~~FluentValidation~~ — простые if проверки достаточно
5. ❌ ~~Refresh Tokens~~ — потом, если клиенты попросят

### **Phase 2: Backend для Manager (1-2 дня) - ФОКУС**

1. ✅ `ManagerAuthController` — Telegram Login Widget валидация
2. ✅ `ManagerTracksController` — CRUD (это база!)
3. ✅ `ManagerStatisticsController` — dashboard stats (менеджеры любят графики)
4. ⚠️ ~~ManagerClientsController~~ — сделаем в Phase 3, если время останется
5. ✅ Авторизация `[Authorize(Roles = "Manager")]`
6. ⚠️ Фильтрация/пагинация — простая, без библиотек

### **Phase 3: Frontend для Manager (2-3 дня) - КИЛЛЕР-ФИЧИ**

**Приоритет 1 (MUST HAVE):**
1. ✅ `TelegramLoginButton.tsx` — кнопка входа с виджетом
2. ✅ `ManagerLayout.tsx` — простой layout с sidebar (Tailwind)
3. ✅ `Dashboard.tsx` — статистика (4 карточки + список)
4. ✅ **`ImportExport.tsx` — ПРИОРИТЕТ! Это киллер-фича**
5. ✅ `TrackForm.tsx` — создание трека (простая форма, useState)

**Приоритет 2 (NICE TO HAVE):**
6. ⚠️ `TrackList.tsx` — список с фильтрами (array.filter)
7. ⚠️ `ClientList.tsx` — если останется время

### **Phase 4: Тестирование (0.5-1 день)**

1. ✅ Вход через Telegram (Client и Manager)
2. ✅ Multi-tenancy изоляция
3. ✅ Импорт Excel
4. ✅ Создание трека

**Итого: ~4-7 дней разработки** (вместо 7-11!)

**Что выкинули:**
- ❌ Email/Password регистрация (2 дня экономии)
- ❌ Сложные библиотеки (1 день на изучение)
- ❌ Избыточный функционал (1 день)

**На чем фокус:**
- ✅ Telegram Auth для всех
- ✅ Excel Import (это продает SaaS!)
- ✅ Простота и скорость

---

## 🔧 Технические детали

### **1. Пагинация и фильтрация**

```csharp
// DTOs для фильтрации
public class TrackFilterDto
{
    public string? Search { get; set; }
    public string? ClientCode { get; set; }
    public TrackStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string OrderBy { get; set; } = "CreatedAt";
    public string OrderDirection { get; set; } = "desc";
}

// Response с пагинацией
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
```

### **2. TrackRepository расширения**

```csharp
public interface ITrackRepository : IRepository<Track>
{
    // Существующие
    Task<IEnumerable<Track>> GetByClientCodeAsync(string clientCode, CancellationToken ct);
    
    // НОВЫЕ для Manager
    Task<PagedResult<Track>> GetPagedAsync(
        TrackFilterDto filter, 
        CancellationToken ct);
    
    Task<IEnumerable<Track>> SearchAsync(
        string searchTerm, 
        CancellationToken ct);
    
    Task<Dictionary<TrackStatus, int>> GetCountByStatusAsync(CancellationToken ct);
    
    Task<int> GetCountByClientCodeAsync(string clientCode, CancellationToken ct);
}
```

### **3. UI библиотеки (рекомендации)**

```bash
# Для улучшения UX
npm install @tanstack/react-table
npm install react-query
npm install react-hook-form
npm install zod
npm install date-fns
npm install react-hot-toast
npm install lucide-react  # иконки
```

### **4. Сайдбар меню (структура)**

```typescript
const menuItems = [
  {
    icon: LayoutDashboard,
    label: 'Dashboard',
    path: '/manager/dashboard',
  },
  {
    icon: Package,
    label: 'Tracks',
    path: '/manager/tracks',
  },
  {
    icon: Users,
    label: 'Clients',
    path: '/manager/clients',
  },
  {
    icon: Upload,
    label: 'Import/Export',
    path: '/manager/import',
  },
  {
    icon: Settings,
    label: 'Settings',
    path: '/manager/settings',
  },
];
```

---

## 📊 Метрики успеха

После завершения разработки должны быть реализованы:

- ✅ Полноценный CRUD для треков
- ✅ Фильтрация, поиск, пагинация
- ✅ Импорт/экспорт Excel
- ✅ Статистика на dashboard
- ✅ Список клиентов с количеством треков
- ✅ Авторизация Manager (email/password)
- ✅ Респонсивный дизайн (desktop-first)
- ✅ Изоляция данных (multi-tenancy)

---

## 🎯 Следующие шаги

1. **Утвердить архитектуру** - проверить что все требования покрыты
2. **Начать рефакторинг** - подготовить код к новым фичам
3. **Создать Backend** - API endpoints для Manager
4. **Создать Frontend** - UI для веб-админки
5. **Тестирование** - проверить все функции
6. **Деплой** - залить на Railway

---

**Готовы начать? 🚀**
