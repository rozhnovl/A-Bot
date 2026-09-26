# Production API

Производственные клиенты отправляют на сервер события. Для данных, доступных через EVE ESI, основной путь — серверная синхронизация ESI; это API остаётся для локальных действий ботов и ручных подтверждений.

## Подключение

В production задайте ключ через секрет или переменную окружения:

```text
Production__ApiKey=<длинный-случайный-ключ>
```

Все запросы клиентов идут на `POST /api/production/events` с заголовком `X-Production-Api-Key`. Ключ обязателен и в Development. Значение в `appsettings.json` оставлено пустым намеренно; задавайте его через секрет/переменную окружения. Не передавайте ключ в URL или репозиторий.

План для скриптов и помощника: `GET /api/production/plan` (тот же заголовок) возвращает `netProfitGoal` (по умолчанию 10 млрд ISK/месяц), `reconciledNetProfit`, `unreconciledSalesCount`, `openProductionNetProfitPotential`, `unpricedOpenWorksCount`, `projectedNetProfit`, `profitGapAfterPipeline`, `requiredNetProfitPerRemainingDay`. `revenueThisMonth` оставлена отдельно как справочная выручка. В `openWorks` и `openPurchases` доступны исходные работы и закупки. `sellOrders` отражает потенциальную **выручку**, а `observedTradingFees` — пока не распределённые дебетовые проводки журнала; ни то ни другое автоматически не прибавляется к чистой прибыли. Прогноз не является фактом.

При подключённом EVE SSO `GET /api/production/esi/status` показывает свежесть загрузок и состояние фонового опроса (`sync`), а `GET /api/production/esi/records?kind=IndustryJob&limit=100` возвращает наблюдения ESI с `items` и `nextBeforeId`. Для следующих страниц передайте `beforeId=<nextBeforeId>`; другие виды: `WalletTransaction`, `WalletJournalEntry`, `Order`, `Asset`. Подключение и пределы расчёта описаны в [рабочем плане](production-workflow.md).

## События

Поддерживаются `resource-received`, `sale`, `production-started`, `production-progress`, `purchase-ordered`.

Пример продажи:

```json
{
  "clientId": "bot-01",
  "eventId": "market-transaction-123456",
  "eventType": "sale",
  "capturedAt": "2026-09-13T11:30:00Z",
  "product": "Плита усиленного корпуса",
  "customer": "Корпорация Polaris",
  "quantity": 4,
  "unitPrice": 19500000,
  "unitCost": 12000000,
  "fees": 600000,
  "otherCosts": 200000
}
```

`unitCost` задаётся за единицу, `fees` и `otherCosts` — за всю продажу. Для включения продажи в факт чистой прибыли все три поля должны быть известны. Настоящий ноль указывайте как `0`; отсутствие поля оставляет продажу несверенной. ESI-продажи приходят без этих затрат, их можно сверить на странице «Продажи». Бот не должен повторно отправлять продажу, уже импортированную через ESI.

Пример получения ресурсов. Для существующего ресурса сервер пересчитает средневзвешенную цену:

```json
{
  "clientId": "bot-01",
  "eventId": "received-mexallon-20260913-1",
  "eventType": "resource-received",
  "resource": "Mexallon",
  "quantity": 30000,
  "unitPrice": 29.9
}
```

Пример обновления стадии производства:

```json
{
  "clientId": "bot-01",
  "eventId": "job-774-progress-86",
  "eventType": "production-progress",
  "product": "Плита усиленного корпуса",
  "progress": 86
}
```

`eventId` должен быть стабильным у одного события. Повторная отправка того же `(clientId, eventId)` не создаёт вторую операцию. `GET /api/production/snapshot` возвращает текущий снимок для начальной синхронизации.

Для обновления открытого дашборда сервер публикует событие `productionChanged` через SignalR hub `/hubs/production`.

Минимальный вызов из .NET-клиента:

```csharp
using var request = new HttpRequestMessage(HttpMethod.Post, "/api/production/events");
request.Headers.Add("X-Production-Api-Key", apiKey);
request.Content = JsonContent.Create(new
{
    clientId = "bot-01",
    eventId = "market-transaction-123456",
    eventType = "sale",
    product = "Плита усиленного корпуса",
    customer = "Корпорация Polaris",
    quantity = 4,
    unitPrice = 19500000m,
    unitCost = 12000000m,
    fees = 600000m,
    otherCosts = 200000m
});
await httpClient.SendAsync(request);
```
