# =========================================
# Stage 1: Build
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы решения и проектов для восстановления зависимостей
COPY ["Cargo.Solution.sln", "./"]
COPY ["src/Cargo.API/Cargo.API.csproj", "src/Cargo.API/"]
COPY ["src/Cargo.Core/Cargo.Core.csproj", "src/Cargo.Core/"]
COPY ["src/Cargo.Infrastructure/Cargo.Infrastructure.csproj", "src/Cargo.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "Cargo.Solution.sln"

# Копируем остальные файлы проекта
COPY . .

# Собираем проект в Release режиме
WORKDIR "/src/src/Cargo.API"
RUN dotnet build "Cargo.API.csproj" -c Release -o /app/build

# =========================================
# Stage 2: Publish
# =========================================
FROM build AS publish
RUN dotnet publish "Cargo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================
# Stage 3: Runtime
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Создаем непривилегированного пользователя для безопасности
RUN addgroup --gid 1000 appuser && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Копируем опубликованное приложение из стадии publish
COPY --from=publish /app/publish .

# Меняем владельца файлов на appuser
RUN chown -R appuser:appuser /app

# Переключаемся на непривилегированного пользователя
USER appuser

# Открываем порт
EXPOSE 8080

# Healthcheck для проверки состояния контейнера
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Точка входа в приложение
ENTRYPOINT ["dotnet", "Cargo.API.dll"]


