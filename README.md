# API Gateway с агрегацией данных

API Gateway (BFF) для агрегации данных из трех микросервисов с кэшированием, отказоустойчивостью и JWT аутентификацией.

## Описание

Проект представляет собой микросервисную архитектуру с централизованным API Gateway, который агрегирует данные из трех независимых микросервисов:

- **ApiGateway** - точка входа для клиентов, агрегирует данные из микросервисов
- **UserService** - управление пользователями
- **OrderService** - управление заказами
- **ProductService** - управление продуктами

Дополнительные компоненты:
- **Redis** - распределенное кэширование
- **Prometheus** - сбор метрик
- **Grafana** - визуализация метрик

## Технологии

- .NET 9.0
- ASP.NET Core Minimal API
- Ocelot 23.2.0 - API Gateway
- gRPC - межсервисная коммуникация
- Polly 8.4.2 - Retry и Circuit Breaker
- Redis - кэширование
- JWT Bearer Authentication
- Serilog - логирование
- Prometheus + Grafana - мониторинг
- Docker Compose

## Структура проекта

```
ServicesGateway/
├── ApiGateway/          # API Gateway
│   ├── Api/            # Обработчики запросов и middleware
│   ├── Application/    # Источники данных и хранилище
│   ├── Domain/         # Доменная логика
│   ├── Composition/    # Конфигурация сервисов
│   └── Services/       # Клиенты к микросервисам
│       ├── Interfaces/ # Интерфейсы клиентов
│       ├── Http/       # HTTP реализации
│       └── Grpc/       # gRPC реализации
├── UserService/        # Микросервис пользователей
├── OrderService/       # Микросервис заказов
├── ProductService/     # Микросервис продуктов
└── prometheus/         # Конфигурация Prometheus
```

## Требования

- Docker Desktop
- .NET SDK 9.0 (для локальной разработки)

## Установка и запуск

### Запуск через Docker Compose

1. Клонируйте репозиторий:
```bash
git clone <repository-url>
cd ServicesGateway
```

2. Запустите все сервисы:
```bash
docker-compose up -d --build
```

3. Проверьте статус:
```bash
docker-compose ps
```

Сервисы будут доступны по адресам:
- ApiGateway: http://localhost:5000
- UserService: http://localhost:5001
- OrderService: http://localhost:5002
- ProductService: http://localhost:5003
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (логин/пароль: admin/admin)

### Остановка

```bash
docker-compose down
```

## Использование API

### 1. Получение JWT токена

```bash
POST http://localhost:5000/api/auth/login?username=alex
```

Ответ:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresInSeconds": 3600
}
```

### 2. Получение профиля пользователя

```bash
GET http://localhost:5000/api/profile/1
Authorization: Bearer <JWT_TOKEN>
```

Ответ содержит агрегированные данные:
```json
{
  "userInfo": {
    "id": 1,
    "firstName": "Александр",
    "lastName": "Кузнецов",
    "emailAddress": "alex.kuznetsov@mail.ru"
  },
  "orderHistory": [
    {
      "orderId": 1,
      "customerId": 1,
      "productId": 201,
      "itemQuantity": 1,
      "orderTotal": 1249.50,
      "createdDateTime": "2025-12-09T16:04:58.3219535+00:00",
      "productInfo": {
        "id": 201,
        "productName": "Игровая консоль",
        "description": "Новейшая игровая консоль с поддержкой 4K",
        "unitPrice": 1249.50
      }
    }
  ],
  "orderCount": 3,
  "totalPurchaseAmount": 3198.49
}
```

### 3. Прямой доступ к микросервисам

UserService:
```bash
GET http://localhost:5001/api/users
GET http://localhost:5001/api/users/1
```

OrderService:
```bash
GET http://localhost:5002/api/orders/user/1
GET http://localhost:5002/api/orders/1
```

ProductService:
```bash
GET http://localhost:5003/api/products
GET http://localhost:5003/api/products/201
```

### 4. Метрики Prometheus

```bash
GET http://localhost:5000/metrics
GET http://localhost:5001/metrics
GET http://localhost:5002/metrics
GET http://localhost:5003/metrics
```

## Проверка работоспособности

### Ручная проверка

1. Проверка доступности сервисов:
```bash
curl http://localhost:5000/metrics
curl http://localhost:5001/api/users
curl http://localhost:5002/api/orders/user/1
curl http://localhost:5003/api/products
```

2. Получение токена и профиля:
```bash
# Получение токена
TOKEN=$(curl -s -X POST "http://localhost:5000/api/auth/login?username=alex" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

# Получение профиля:
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/profile/1
```

3. Проверка кэширования:
```bash
time curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/profile/1
```

4. Проверка rate limiting:
```bash
# Отправка множества запросов (лимит 120 в минуту)
for i in {1..130}; do
  curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/profile/1
done
# После 120 запросов должен вернуться 429
```

## Конфигурация

### ApiGateway (appsettings.json)

Основные настройки:
- `Services:UseGrpc` - использовать gRPC для внутренних вызовов (true/false)
- `Redis:ConnectionString` - строка подключения к Redis
- `Jwt:Key` - секретный ключ для JWT
- `Jwt:Issuer` - издатель токена
- `Jwt:Audience` - аудитория токена

### Параметры устойчивости

Настроены в коде (ServiceRegistration.cs):
- Retry: 4 попытки с экспоненциальной задержкой
- Circuit Breaker: 6 событий до разрыва, 45 секунд разрыва
- HTTP Timeout: 6-8 секунд в зависимости от сервиса

### Rate Limiting

- Лимит: 120 запросов в минуту на IP
- Окно: 60 секунд
- Ответ при превышении: HTTP 429

### Кэширование

- Полный профиль: TTL 45 секунд
- Частичный профиль: TTL 20 секунд
- Ключ кэша: `user_profile_cache:{userId}`

## Мониторинг

### Prometheus

URL: http://localhost:9090

Prometheus автоматически собирает метрики со всех сервисов. Конфигурация находится в `prometheus/prometheus.yml`.

### Grafana

URL: http://localhost:3000
Логин/Пароль: admin/admin

Для настройки:
1. Перейдите в Configuration → Data Sources
2. Нажмите "Add data source"
3. Выберите "Prometheus"
4. В поле "URL" укажите: `http://prometheus:9090`
5. Нажмите "Save & Test" - должно появиться сообщение "Data source is working"

Для проверки доступных метрик в Prometheus:
- Откройте http://localhost:9090
- Перейдите в раздел "Status" → "Target health" - все targets должны быть в состоянии "UP"
- В разделе "Graph" можно выполнить запросы, например:
  - `http_request_duration_seconds_count` - количество HTTP запросов
  - `http_request_duration_seconds_sum` - суммарное время выполнения запросов
  - `rate(http_request_duration_seconds_count[5m])` - скорость запросов за 5 минут

## Основные возможности

### Агрегация данных

ApiGateway объединяет данные из трех микросервисов в один ответ. При запросе профиля пользователя:
1. Получает данные пользователя из UserService
2. Получает заказы из OrderService
3. Получает информацию о продуктах из ProductService
4. Объединяет все в один объект

### Кэширование

Результаты агрегации кэшируются в Redis:
- Первый запрос идет к микросервисам
- Последующие запросы берутся из кэша
- TTL зависит от полноты данных

### Отказоустойчивость

Используется библиотека Polly:
- **Retry** - автоматические повторы при временных ошибках
- **Circuit Breaker** - временная блокировка запросов при множественных ошибках
- **Fallback** - возврат частичных данных из кэша при недоступности сервисов

### Безопасность

- JWT аутентификация для защищенных эндпоинтов
- Rate limiting для защиты от перегрузки
- Валидация токенов на каждом запросе

### gRPC коммуникация

Внутренние вызовы между ApiGateway и микросервисами выполняются через gRPC для лучшей производительности. Можно переключиться на HTTP через настройку `Services:UseGrpc=false`.

## Разработка

### Локальная разработка

1. Запустите Redis:
```bash
docker run -d -p 6379:6379 redis:7-alpine
```

2. Запустите сервисы отдельно:
```bash
# Терминал 1
cd UserService
dotnet run

# Терминал 2
cd OrderService
dotnet run

# Терминал 3
cd ProductService
dotnet run

# Терминал 4
cd ApiGateway
dotnet run
```

### Сборка

```bash
dotnet build
```

### Очистка

```bash
dotnet clean
docker-compose down -v
```

## Архитектура

Проект использует Clean Architecture с разделением на слои:

- **Domain** - доменная логика и бизнес-правила
- **Application** - источники данных и хранилище
- **API** - обработчики запросов и middleware
- **Composition** - конфигурация и регистрация сервисов

В микросервисах:
- **Core** - доменные сущности и интерфейсы
- **Infrastructure** - реализации хранилищ и метрики
- **Api** - REST контроллеры и gRPC обработчики

