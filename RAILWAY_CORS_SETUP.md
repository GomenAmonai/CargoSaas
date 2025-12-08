# 🚨 CORS Setup для Railway Production

## Проблема

Frontend на `https://gentle-comfort-production-da23.up.railway.app` не может обратиться к Backend из-за CORS политики.

## Решение

### **1. Добавь переменные окружения в Railway Backend Service:**

Railway → Backend Service → Variables → Add Variable:

```bash
Cors__AllowedOrigins__0=https://gentle-comfort-production-da23.up.railway.app
Cors__AllowedOrigins__1=https://web.telegram.org
Cors__AllowedOrigins__2=https://webk.telegram.org
Cors__AllowedOrigins__3=https://webz.telegram.org
```

**ВАЖНО:** 
- Используй точный URL фронтенда (как в ошибке браузера)
- Проверь что URL начинается с `https://`
- НЕ добавляй слэш в конце (`/`)

### **2. После добавления переменных:**

1. **Redeploy Backend Service** на Railway
2. Проверь логи - должны появиться разрешенные домены
3. Попробуй залогиниться снова

### **3. Проверка что работает:**

В браузере (DevTools → Network):
- Запрос к `/api/client/auth` должен вернуть статус 200 (не CORS ошибку)
- Response Headers должны содержать: `Access-Control-Allow-Origin: https://gentle-comfort-production-da23.up.railway.app`

---

## Альтернатива: Если домен фронтенда меняется

Если Railway генерирует случайные домены при каждом деплое, можно временно использовать:

```bash
# В Railway Backend Variables
Cors__AllowedOrigins__0=*
```

**НО:** Это небезопасно для production! Используй только для тестирования.

---

## Проверка текущей конфигурации

После деплоя проверь логи Railway:
```
info: Cargo.API.Program[0]
      CORS configured with origins: https://gentle-comfort-production-da23.up.railway.app, https://web.telegram.org, ...
```

Если видишь эту строку - CORS настроен правильно.
