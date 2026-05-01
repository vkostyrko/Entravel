# Entravel
Entravel test task

## Running locally with Docker

### Prerequisites

- Docker Desktop

### Commands

```bash
docker compose up --build
```

```bash
docker compose down -v
```

### What happens in Docker / local dev environments

When `ASPNETCORE_ENVIRONMENT` is `Docker` (or `Development` / `Local`), the API:

- applies EF Core migrations automatically on startup
- seeds local-only test data for `Customers` and `Inventory` (idempotent; container restarts won’t duplicate data)

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

**Invalid request**: returns `400` with validation details.

### Inspecting the database (PostgreSQL)

Postgres is exposed on `localhost:5432` with:

- database: `entravel`
- user: `postgres`
- password: `postgres`

Useful SQL:

```sql
select * from "Customers" order by "CreatedDate" desc;
select * from "Inventory" order by "CreatedDate" desc;

select * from "Orders" order by "CreatedDate" desc;
select * from "OrderItems" order by "CreatedDate" desc;
select * from "OutboxMessages" order by "CreatedDate" desc;
```

### Expected results checklist

- `docker compose up --build` starts both containers successfully
- API is reachable on `http://localhost:8080`
- `GET /health` returns `200 OK`
- Seed data exists in `Customers` and `Inventory`
- Valid `POST /api/orders` returns `202 Accepted`
- Invalid `POST /api/orders` returns `400`
- Database contains:
  - a new row in `Orders`
  - a new row in `OutboxMessages` created in the same request flow
