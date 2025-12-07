# 🧪 Local Testing Guide

> Всегда тестируем локально перед push в GitHub (чтобы не создавать лишние деплои)

---

## 🚀 Quick Start (рекомендуемый способ)

### **1. Автоматическая проверка**

```bash
# Запусти все проверки одной командой
./test-local.sh

# Проверяет:
# ✅ Backend build
# ✅ Frontend build  
# ✅ TypeScript errors
# ✅ ESLint
```

Если всё зелёное ✅ → можно пушить!

---

## 🔧 Ручное тестирование

### **Backend**

```bash
cd src/Cargo.API

# Build
dotnet build

# Run
dotnet run

# Проверь здоровье
curl http://localhost:8080/health
# Ответ: {"status":"Healthy"}

# Swagger UI
open http://localhost:8080/swagger
```

---

### **Frontend**

```bash
cd src/Cargo.Web

# Установи зависимости (если нужно)
npm install

# Build check
npm run build

# TypeScript check
npx tsc --noEmit

# Lint
npm run lint

# Dev server (для разработки)
npm run dev
# → http://localhost:5173
```

---

## 🎯 Тестирование Telegram WebApp локально

### **Проблема:**
Telegram WebApp можно открыть только через бота, который указывает на production URL.

### **Решение 1: ngrok (рекомендую)**

```bash
# 1. Установи ngrok
brew install ngrok

# 2. Запусти frontend
cd src/Cargo.Web
npm run dev

# 3. В другом терминале - туннель
ngrok http 5173

# 4. Скопируй URL (например: https://abc123.ngrok-free.app)

# 5. Обнови WebApp URL в боте
# @BotFather → /mybots → [твой бот] → Bot Settings → Menu Button → Edit URL
# Вставь ngrok URL

# 6. Открой бота в Telegram и тестируй!
```

---

### **Решение 2: Production Backend + Local Frontend**

```bash
# Создай .env.local
cd src/Cargo.Web
echo "VITE_API_URL=https://cargosaas-production.up.railway.app/api" > .env.local

npm run dev

# Затем ngrok (см. выше)
```

---

### **Решение 3: Полный локальный стек**

```bash
# Terminal 1: Backend
cd src/Cargo.API
dotnet run

# Terminal 2: Frontend
cd src/Cargo.Web
echo "VITE_API_URL=http://localhost:8080/api" > .env.local
npm run dev

# Terminal 3: ngrok
ngrok http 5173

# Обнови bot WebApp URL → ngrok URL
```

---

## 📋 Pre-Push Checklist

Перед каждым `git push`:

```bash
# 1. Запусти автопроверку
./test-local.sh

# Если всё ОК:

# 2. Проверь что изменилось
git status
git diff

# 3. Коммит
git add .
git commit -m "feat: ваше описание"

# 4. ПЕРЕД PUSH - последняя проверка
git log -1  # Посмотри что коммитишь

# 5. Push
git push origin main
```

---

## 🐛 Troubleshooting

### **"dotnet: command not found"**

```bash
# Установи .NET 8 SDK
brew install dotnet@8
```

---

### **"npm: command not found"**

```bash
# Установи Node.js 20+
brew install node@20
```

---

### **Frontend build fails с ошибками TypeScript**

```bash
# Проверь ошибки
cd src/Cargo.Web
npx tsc --noEmit

# Исправь ошибки перед push!
```

---

### **ngrok показывает "Visit site" экран**

Это нормально для бесплатного плана. Просто кликни "Visit site" один раз.

Или используй платный ngrok для постоянного домена.

---

### **Telegram бот не открывает WebApp**

1. Проверь что WebApp URL обновлён в @BotFather
2. Проверь что ngrok туннель работает (открой URL в браузере)
3. Рестартни бота: `/start`

---

## 🎯 Workflow для новых фич

```bash
# 1. Создай фичу
# ... пиши код ...

# 2. Проверь локально
./test-local.sh

# 3. Тестируй через ngrok
ngrok http 5173
# Обнови bot URL → открой в Telegram

# 4. Если всё работает → коммит
git add .
git commit -m "feat: ..."

# 5. Push → Railway задеплоит
git push origin main

# 6. Верни bot URL обратно на production
# @BotFather → Edit URL → https://gentle-comfort-production-da23.up.railway.app
```

---

## 💡 Pro Tips

### **Hot Reload для Backend**

```bash
# Установи dotnet-watch
dotnet tool install -g dotnet-watch

# Запусти с hot reload
cd src/Cargo.API
dotnet watch run
```

---

### **Постоянный ngrok домен**

Создай аккаунт на ngrok.com → бесплатный static domain

```bash
ngrok config add-authtoken <your-token>
ngrok http 5173 --domain=your-static-domain.ngrok-free.app
```

---

### **Тестирование API без Telegram**

```bash
# Получи initData из браузера DevTools
# (открой WebApp в Telegram Desktop → F12 → Console)
console.log(Telegram.WebApp.initData)

# Используй в Postman/curl
curl -X POST http://localhost:8080/api/client/auth \
  -H "Content-Type: application/json" \
  -d '{"initData":"query_id=AAH..."}'
```

---

## ✅ Summary

**До каждого push:**
1. `./test-local.sh` ✅
2. Проверь изменения `git diff`
3. Commit + Push

**Для тестирования WebApp:**
1. `npm run dev` (frontend)
2. `ngrok http 5173`
3. Обнови bot URL
4. Тестируй в Telegram

**Готово!** 🚀




