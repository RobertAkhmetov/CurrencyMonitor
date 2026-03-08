# CurrencyMonitor

Решение на .NET 8 с микросервисной архитектурой:

- `DatabaseMigrator` для миграции БД Postgres
- `CurrencyRates.Worker` для фонового обновления курсов из ЦБ РФ
- `UserService` для регистрации, логина, логаута и управления избранными валютами
- `FinanceService` для получения курсов избранных валют пользователя
- `ApiGateway` (YARP) для единой точки входа
- unit-тесты для `UserService.Application` и `FinanceService.Application`

## Структура

- `src/Shared/Persistence` — EF Core `DbContext`, сущности и миграции
- `src/Services/UserService` — Clean Architecture + CQRS
- `src/Services/FinanceService` — Clean Architecture + CQRS
- `src/Infrastructure/DatabaseMigrator` — сервис миграции
- `src/Infrastructure/CurrencyRates.Worker` — фоновый сервис курсов
- `src/ApiGateway/ApiGateway` — API Gateway
- `tests` — unit-тесты

## Требования

- .NET SDK 8
- PostgreSQL 16+ или Docker

## Быстрый старт

1. Поднять Postgres:

```bash
docker compose up -d
```

2. Применить миграции:

```bash
dotnet run --project src/Infrastructure/DatabaseMigrator/DatabaseMigrator.csproj
```

3. Запустить `UserService`:

```bash
dotnet run --project src/Services/UserService/UserService.Api/UserService.Api.csproj
```

4. Запустить `FinanceService`:

```bash
dotnet run --project src/Services/FinanceService/FinanceService.Api/FinanceService.Api.csproj
```

5. Запустить gateway:

```bash
dotnet run --project src/ApiGateway/ApiGateway/ApiGateway.csproj
```

6. Запустить фоновый сервис:

```bash
dotnet run --project src/Infrastructure/CurrencyRates.Worker/CurrencyRates.Worker.csproj
```

## Порты

- API Gateway: `http://localhost:5000`
- UserService: `http://localhost:5001`
- FinanceService: `http://localhost:5002`

## Основные эндпоинты через Gateway

- `POST /api/user/register`
- `POST /api/user/login`
- `POST /api/user/logout` (JWT)
- `PUT /api/user/favorites` (JWT)
- `GET /api/finance/rates` (JWT)

## Формат запросов

Регистрация:

```json
{
  "name": "alice",
  "password": "qwerty",
  "favorites": ["USD", "EUR", "CNY"]
}
```

Логин:

```json
{
  "name": "alice",
  "password": "qwerty"
}
```

Обновление избранных:

```json
{
  "favorites": ["USD", "KZT", "JPY"]
}
```

## Тесты

```bash
dotnet test CurrencyMonitor.sln
```

## Сборка

```bash
dotnet build CurrencyMonitor.sln
```
