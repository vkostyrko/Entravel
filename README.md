# Entravel
Entravel test task

## Running locally with Docker

### Prerequisites

- Docker Desktop

### Swagger quick demo

1. Start Docker:

```bash
docker compose up --build
```

2. Open Swagger:

`http://localhost:8080/swagger`

3. Submit `POST /api/orders` using seeded values:

- Customer: `11111111-1111-1111-1111-111111111111`
- Inventory: `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`
- Example: `totalAmount = 100`, `discount = 10`

4. Copy the returned `orderId`.

5. Call `GET /api/orders/{orderId}`.

Expected:

- Initially it may be `Pending` / `Processing`
- After a moment it should become `Processed`
- `TotalAmount = 100`
- `Discount = 10`
- `FinalAmount = 90`

Note:

- Customer/Inventory existence checks against the database are a real-world requirement.
- For this interview task, FluentValidation covers request shape/ranges; seeded IDs are documented for local testing.

### Commands

```bash
docker compose up --build
```

```bash
docker compose down -v
```

### What happens in Docker / local dev environments

When `ASPNETCORE_ENVIRONMENT` is `Docker` (or `Development` / `Local`):

- **API** (`InitializeAsync`): applies EF Core migrations on startup, then seeds local-only `Customers` and `Inventory` (idempotent; container restarts won’t duplicate data).
- **Outbox Worker** (`ApplyMigrationsAsync` only): applies migrations on startup so the schema exists; it does **not** seed test data.
- **Order Processing Worker** (`ApplyMigrationsAsync` only): same migration-only startup as the outbox worker.

### End-to-end async order flow

1. **API** receives `CreateOrder` (`POST /api/orders`).
2. **API** stores a new `Order` (status `Pending`) and an `OutboxMessage` in **PostgreSQL in one transaction**.
3. **Outbox Worker** **polls** the `OutboxMessages` table, claims rows, publishes `OrderSubmitted` JSON to RabbitMQ exchange **`entravel.events`** (type **topic**, routing key **`orders.submitted`**), then marks the outbox row **`Sent`** when publish succeeds.
4. **Order Processing Worker** does **not** poll RabbitMQ. It hosts an **event-driven** `AsyncEventingBasicConsumer`: RabbitMQ **pushes** deliveries as soon as messages arrive on queue **`entravel.order-processing`** (bound to `entravel.events` with routing key `orders.submitted`). This is analogous to a queue-triggered **Azure Function** staying alive and reacting to each delivery.
5. The worker’s **dispatcher** routes by message type (`OrderSubmitted` from the AMQP `type` property and/or subscription config) to **`OrderSubmittedMessageHandler`**.
6. The handler runs **business logic** (simulated delay, validation) via **`IOrderProcessingRepository`**. The repository loads the order and calls **domain behavior** (no direct property mutation) to move the order **`Pending` → `Processing` → `Processed`** (with **`UpdatedDate`** set). A row lock (`SELECT ... FOR UPDATE`) is used so only one concurrent consumer processes a given order at a time.
7. The RabbitMQ message is **acknowledged only after** the database outcome is known (success, already processed, or poison path). Transient failures use **nack + requeue**; invalid JSON / unknown type / missing order (poison) use **nack without requeue**.

**Delivery note:** RabbitMQ delivery is **at-least-once**. The outbox publisher can also retry and duplicate publishes. The consumer is **idempotent**:

- If `Order.Status` is already **`Processed`**, the handler returns success and the message is **acked** (no duplicate side effects).
- For interview scope, **status-based idempotency** is sufficient (no separate `ProcessedMessages` table). A stronger approach is to add a **`ProcessedMessages`** table keyed by **`MessageId`** / **`OutboxMessage.Id`** if you need deduplication beyond order status.

**Outbox vs RabbitMQ:** The **Outbox Worker** polls PostgreSQL. The **Order Processing Worker** listens on an open AMQP consumer channel; it never polls the broker for messages.

### Seeded values (stable IDs)

**Customers**

- `11111111-1111-1111-1111-111111111111` (Acme Travel, `acme@example.com`)
- `22222222-2222-2222-2222-222222222222` (Globex Corp, `globex@example.com`)
- `33333333-3333-3333-3333-333333333333` (Umbrella Ltd, `umbrella@example.com`)

**Inventory**

- `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` (City tour)
- `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb` (Airport transfer)
- `cccccccc-cccc-cccc-cccc-cccccccccccc` (Museum ticket)

### Example requests

- Use `Entravel.API/Entravel.API.http` (recommended), or call the API directly at `http://localhost:8080`.

**Valid request**: returns `202 Accepted`, and persists both an `Orders` row and an `OutboxMessages` row (same transaction).

Example payload (discount is applied asynchronously by the worker, not by the API):

```json
{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "items": [
    { "inventoryId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "quantity": 1 }
  ],
  "totalAmount": 100.00,
  "discount": 10
}
```

**Invalid request**: returns `400` with validation details.

### Inspecting the database (PostgreSQL)

Postgres is exposed on `localhost:5432` with:

- database: `entravel`
- user: `postgres`
- password: `postgres`

Useful SQL:

```sql
SELECT * FROM "Customers" ORDER BY "CreatedDate" DESC;
SELECT * FROM "Inventory" ORDER BY "CreatedDate" DESC;

SELECT * FROM "Orders" ORDER BY "CreatedDate" DESC;
SELECT * FROM "OrderItems" ORDER BY "CreatedDate" DESC;
SELECT * FROM "OutboxMessages" ORDER BY "CreatedDate" DESC;

SELECT "Id", "Status", "TotalAmount", "Discount", "FinalAmount", "CreatedDate", "UpdatedDate"
FROM "Orders"
ORDER BY "CreatedDate" DESC;
```

### Expected results checklist

- `docker compose up --build` starts **postgres**, **rabbitmq**, **api**, **outbox-worker**, and **order-processing-worker**
- API is reachable on `http://localhost:8080`
- `GET /health` returns `200 OK`
- Seed data exists in `Customers` and `Inventory`
- Valid `POST /api/orders` returns `202 Accepted`
- Invalid `POST /api/orders` returns `400`
- Database contains:
  - a new row in `Orders` with status `Pending`, then **`Processed`** after the message is handled
  - a new row in `OutboxMessages` created in the same request flow, later status **`Sent`**
- RabbitMQ Management UI shows:
  - exchange **`entravel.events`**
  - queue **`entravel.order-processing`**
  - binding **`entravel.events`** → **`entravel.order-processing`** with routing key **`orders.submitted`**
- Publishing the same `OrderSubmitted` payload again does **not** re-run processing once the order is **`Processed`** (duplicate deliveries are acked).

## Outbox Publisher + RabbitMQ + Order processing

### Architecture

- **API** writes `Orders` + `OutboxMessages` in the same DB transaction (atomic commit).
- **Outbox Worker** polls PostgreSQL, claims rows safely, publishes to RabbitMQ, and updates message status.
- **RabbitMQ** is the broker; reusable **consumer infrastructure** lives in **`Entravel.Rmq`** (`RabbitMqConsumerHostedService`, subscriptions, dispatcher hook).
- **Order Processing Worker** is a separate process/container: EF + `IRabbitMqMessageDispatcher` + `OrderSubmittedMessageHandler`.

### Delivery semantics

The publisher implements **Transactional Outbox** with **at-least-once delivery**.

In rare crash scenarios it’s possible for the worker to successfully publish to RabbitMQ and crash before the DB row is marked as `Sent`, causing a retry and **duplicate delivery**.

**Consumers must be idempotent** (here: **`Order.Status == Processed`**, optionally extended with a **`ProcessedMessages`** table keyed by message id).

### Local run + UI

`docker compose up --build` starts:

- PostgreSQL
- RabbitMQ (`rabbitmq:3-management`)
- API
- Outbox Worker
- Order Processing Worker

RabbitMQ Management UI: `http://localhost:15672`  
Credentials: `guest` / `guest`

### Test steps

1. Start: `docker compose up --build`
2. `curl http://localhost:8080/health` (or use `.http` file)
3. Submit an order (see `Entravel.API/Entravel.API.http`)
4. Verify in DB that a row exists in `"OutboxMessages"` with `Status = New`
5. Wait up to the poll interval (default 5s)
6. Verify the row becomes `Status = Sent` and `SentDate` is populated
7. Verify `"Orders"."Status"` becomes **`Processed`** after the order worker consumes the message

Useful SQL:

```sql
SELECT *
FROM "OutboxMessages"
ORDER BY "CreatedDate" DESC;
```

### Verification script (acceptance)

1. `docker compose down -v`
2. `docker compose up --build`
3. `GET http://localhost:8080/health` → `200`
4. RabbitMQ UI: exchange `entravel.events`, queue `entravel.order-processing`, binding key `orders.submitted`
5. Valid `CreateOrder` → order row + outbox row
6. Outbox worker → outbox `Sent`
7. Order processing worker → order `Processed`
8. Duplicate `OrderSubmitted` (republish same body) → still `Processed`, no double processing
9. `dotnet build Entravel.slnx` succeeds
