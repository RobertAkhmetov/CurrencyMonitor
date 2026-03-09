# CurrencyMonitor

Сделано на .NET 8 с микросервисной архитектурой.

## Структура

- `src/Shared/Persistence` — EF Core `DbContext`, сущности и миграции
- `src/Services/UserService` — сервис пользователей (Clean Architecture + CQRS)
- `src/Services/FinanceService` — сервис финансов (Clean Architecture + CQRS)
- `src/Infrastructure/DatabaseMigrator` — сервис миграции БД
- `src/Infrastructure/CurrencyRates.Worker` — фоновый сервис обновления курсов валют
- `src/ApiGateway/ApiGateway` — API Gateway
- `tests` — unit-тесты


## Подробнее по структуре
Инфраструктура уровня решения (src/Infrastructure)
- `DatabaseMigrator` для миграции БД Postgres
- `CurrencyRates.Worker` для фонового обновления курсов из ЦБ РФ

Обособленные сервисы
- `UserService` для регистрации, логина, логаута и управления избранными валютами
- `FinanceService` для получения курсов избранных валют пользователя
- `ApiGateway` (YARP) для единой точки входа
- unit-тесты для `UserService.Application` и `FinanceService.Application`

## Как запустить все сервисы (All services), чтоб протестировать-оценить работу

1. **Перед запуском всех сервисов обязательно применить миграции БД**:

```bash
dotnet run --project src/Infrastructure/DatabaseMigrator/DatabaseMigrator.csproj
```

2. **Запустить все сервисы (UserService + FinanceService + ApiGateway + CurrencyRates.Worker)** 
- в VS выбрать профиль "AllServices..." и запустить 
- вариант 2 через `dotnet run`:

```bash
dotnet run --project src/Services/UserService/UserService.Api/UserService.Api.csproj
dotnet run --project src/Services/FinanceService/FinanceService.Api/FinanceService.Api.csproj
dotnet run --project src/ApiGateway/ApiGateway/ApiGateway.csproj
dotnet run --project src/Infrastructure/CurrencyRates.Worker/CurrencyRates.Worker.csproj
```

## Требования

- .NET SDK 8
- PostgreSQL 17 или Docker с контейнером Postgre данной версии (по умолчанию сервисы используют адрес БД localhost:5432, чтобы изменить адрес, нужно внести правки в appsettings.json нужного сервиса)

## Запустить сервисы по отдельности

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
