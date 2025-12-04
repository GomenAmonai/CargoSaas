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

# Скрипт для динамической настройки порта
RUN echo '#!/bin/sh\n\
PORT=${PORT:-80}\n\
echo "server { \
    listen $PORT; \
    server_name _; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files \$uri \$uri/ /index.html; \
    } \
}" > /etc/nginx/conf.d/default.conf\n\
nginx -g "daemon off;"' > /start.sh && chmod +x /start.sh

# Открываем порт (динамический)
EXPOSE 80

# Запускаем nginx через скрипт
CMD ["/start.sh"]

