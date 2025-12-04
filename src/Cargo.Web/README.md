# Cargo.Web - Telegram WebApp Frontend

React + TypeScript + Vite фронтенд для Telegram WebApp.

## 🚀 Быстрый старт

### Локальная разработка

```bash
# Установить зависимости
npm install

# Запустить dev сервер
npm run dev
```

Приложение откроется на `http://localhost:5173`

### Настройка

1. Создай `.env` файл:
```env
VITE_API_URL=https://your-railway-app.up.railway.app/api
```

2. Для тестирования в Telegram:
   - Используй ngrok или Railway для публичного URL
   - Настрой WebAppUrl в BotFather

## 📦 Технологии

- **React 18** - UI библиотека
- **TypeScript** - Типизация
- **Vite** - Сборщик
- **Tailwind CSS** - Стилизация
- **@twa-dev/sdk** - Telegram WebApp SDK
- **Axios** - HTTP клиент

## 📁 Структура проекта

```
src/
├── api/
│   └── client.ts          # Axios client с interceptors
├── contexts/
│   └── TelegramProvider.tsx # Telegram SDK контекст
├── pages/
│   └── Home.tsx           # Главная страница
├── App.tsx                # Root компонент
├── main.tsx               # Entry point
└── index.css              # Global styles + Tailwind
```

## 🎨 Telegram Theme

Приложение автоматически использует цветовую схему Telegram:
- `bg-tg-bg` - Background color
- `text-tg-text` - Text color
- `bg-tg-button` - Button color
- И другие...

## 🔐 Авторизация

Каждый запрос к API автоматически включает `X-Telegram-Init-Data` header с данными авторизации.

## 🏗️ Build для production

```bash
npm run build
```

Результат в папке `dist/`

## 🚀 Deploy на Railway/Vercel

1. Build проект
2. Deploy папку `dist/`
3. Настрой environment variable `VITE_API_URL`
4. Обнови WebAppUrl в BotFather
