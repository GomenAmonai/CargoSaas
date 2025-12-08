# 🔒 CORS Configuration Guide

## Что изменилось

Вместо небезопасного `AllowAnyOrigin()` теперь используется **белый список** конкретных доменов.

---

## Конфигурация

### **Development (appsettings.Development.json)**

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",  // Vite dev server
      "http://localhost:3000",  // CRA / Next.js
      "http://localhost:5174",
      "http://127.0.0.1:5173"
    ]
  }
}
```

**Поведение:** В Development режиме используется политика `AllowAll` для удобства разработки.

---

### **Production (appsettings.json)**

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000",
      "https://cargosaas.vercel.app",    // Твой фронтенд
      "https://web.telegram.org"         // Telegram Desktop Web
    ]
  }
}
```

**Поведение:** В Production режиме используется политика `AllowSpecificOrigins` с белым списком.

---

## Настройка для Railway

### **Добавить переменные окружения:**

Railway → Backend Service → Variables:

```bash
Cors__AllowedOrigins__0=https://твой-фронтенд.railway.app
Cors__AllowedOrigins__1=https://web.telegram.org
Cors__AllowedOrigins__2=https://твой-кастомный-домен.com
```

**Формат:** .NET использует `__` (двойное подчеркивание) для вложенных секций и `__N` для массивов.

Пример преобразования:
```json
{
  "Cors": {
    "AllowedOrigins": ["domain1", "domain2"]
  }
}
```
↓
```
Cors__AllowedOrigins__0=domain1
Cors__AllowedOrigins__1=domain2
```

---

## Проверка

### **В браузере (DevTools Console):**

```javascript
fetch('https://твой-backend.railway.app/health', {
  method: 'GET',
  headers: {
    'Origin': 'https://твой-фронтенд.railway.app'
  }
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

**Ожидаемый результат:**
- ✅ Status 200 OK
- ✅ Response Headers содержат: `Access-Control-Allow-Origin: https://твой-фронтенд.railway.app`

**Если ошибка CORS:**
- ❌ `Access to fetch at '...' has been blocked by CORS policy`
- 👉 Проверь переменные окружения на Railway
- 👉 Убедись что URL точно совпадает (https vs http, с/без www)

---

## Безопасность

### **Что изменилось:**

| Было | Стало |
|------|-------|
| `AllowAnyOrigin()` | `WithOrigins(allowedOrigins)` |
| Любой домен может обращаться | Только белый список |
| ❌ Небезопасно | ✅ Безопасно |

### **Почему это важно:**

1. **XSS защита** - злоумышленник не может делать запросы с произвольных доменов
2. **CSRF защита** - cookies/credentials работают только с доверенными доменами
3. **Compliance** - соответствие стандартам безопасности для B2B SaaS

---

## Troubleshooting

### **Проблема 1: "CORS policy: No 'Access-Control-Allow-Origin' header"**

**Причина:** Фронтенд домен не в белом списке.

**Решение:**
1. Проверь точный URL фронтенда (включая протокол https://)
2. Добавь его в `Cors:AllowedOrigins` на Railway
3. Рестарт бэкенда

---

### **Проблема 2: "CORS policy: Credentials mode is 'include'"**

**Причина:** Используется `.AllowAnyOrigin()` с `.AllowCredentials()` одновременно (запрещено спецификацией).

**Решение:** Уже исправлено - используем `WithOrigins()`.

---

### **Проблема 3: "Preflight request doesn't pass"**

**Причина:** Браузер отправляет OPTIONS запрос, который блокируется.

**Решение:**
```csharp
policy.WithOrigins(allowedOrigins)
      .AllowAnyMethod()         // ✅ Разрешает OPTIONS
      .AllowAnyHeader()
      .AllowCredentials();
```

Уже настроено в `Program.cs`.

---

## Для разных окружений

### **Local → Railway Backend**

Frontend локально, Backend на Railway:

```json
// appsettings.Development.json на Railway
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  }
}
```

---

### **Vercel Frontend → Railway Backend**

```bash
# Railway Environment Variables
Cors__AllowedOrigins__0=https://твой-проект.vercel.app
Cors__AllowedOrigins__1=https://твой-проект-git-main.vercel.app
Cors__AllowedOrigins__2=https://web.telegram.org
```

---

## Telegram WebApp особенности

Telegram WebApp может открываться из:
- `https://web.telegram.org` (Desktop Web)
- Native apps (iOS/Android) - не требуют CORS
- Telegram Desktop - не требует CORS

**Рекомендация:** Добавь `https://web.telegram.org` в белый список для Desktop Web версии.

---

**✅ Теперь твой API безопасен и соответствует лучшим практикам!**
