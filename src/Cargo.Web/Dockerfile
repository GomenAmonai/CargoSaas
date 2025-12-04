# ========================================
# Build Stage
# ========================================
FROM node:20-alpine AS builder

WORKDIR /app

# Копируем package.json и package-lock.json
COPY package*.json ./

# Устанавливаем зависимости
RUN npm ci

# Копируем исходный код
COPY . .

# Аргумент для API URL (передается при build)
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

# Собираем production build
RUN npm run build

# ========================================
# Production Stage (статический файловый сервер)
# ========================================
FROM nginx:alpine

# Копируем собранные файлы из builder
COPY --from=builder /app/dist /usr/share/nginx/html

# Копируем nginx конфигурацию для SPA (React Router)
# Railway ожидает порт 8080 по умолчанию
RUN echo 'server { \
    listen 8080; \
    server_name _; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

# Открываем порт 8080
EXPOSE 8080

# Запускаем nginx
CMD ["nginx", "-g", "daemon off;"]

