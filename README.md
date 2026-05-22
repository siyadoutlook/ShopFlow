# ShopFlow

A hands-on .NET 10 microservices project exploring production-grade distributed systems patterns — YARP API Gateway, gRPC, MassTransit, RabbitMQ, Outbox pattern, Polly resilience, and OpenTelemetry observability. Run the entire stack with a single Docker Compose command.

---

## Architecture

```
Client
  │
  ▼
ShopFlow.Gateway        (YARP Reverse Proxy — port 5120)
  │
  ├──► ShopFlow.OrderService       (REST API — port 5001)
  │         │
  │         ├──► ShopFlow.InventoryService  (gRPC — port 5102)
  │         │         └── Polly: retry + circuit breaker + timeout
  │         │
  │         └──► RabbitMQ  (publishes OrderConfirmed via Outbox)
  │
  └──► ShopFlow.InventoryService   (REST API — port 5002, gRPC — port 5102)

RabbitMQ
  ├──► ShopFlow.InventoryService    (reduces stock)
  └──► ShopFlow.NotificationService (sends confirmation email)

Observability
  └──► Aspire Dashboard (traces, metrics, logs — port 18888)
```

---

## Projects

| Project | Type | Responsibility |
|---|---|---|
| `ShopFlow.Gateway` | ASP.NET Web API | YARP reverse proxy — single entry point for all traffic |
| `ShopFlow.OrderService` | ASP.NET Web API | Order management, gRPC client, MassTransit publisher |
| `ShopFlow.InventoryService` | ASP.NET Web API | Product stock management, gRPC server, MassTransit consumer |
| `ShopFlow.NotificationService` | Worker Service | Email dispatch, MassTransit consumer |
| `ShopFlow.Shared` | Class Library | Shared proto contracts, events, and observability extensions |

---

## Patterns and Technologies

### Phase 1 — API Gateway + REST
- **YARP** (Yet Another Reverse Proxy) — config-driven routing, single entry point, load balancing ready
- Two downstream services with static in-memory data stores
- Route/cluster separation — external REST traffic routed via gateway, services isolated internally

### Phase 2 — gRPC Service Communication
- **gRPC** with Protobuf contracts defined in `ShopFlow.Shared/Protos/inventory.proto`
- Order Service calls Inventory Service via gRPC to verify stock before confirming an order
- Separate HTTP/1 (REST) and HTTP/2 (gRPC) ports on Inventory Service
- Generated client/server code via `Grpc.Tools` — no manual plumbing

### Phase 3 — Async Messaging + Outbox Pattern
- **RabbitMQ** as the message broker
- **MassTransit** for publisher/consumer abstraction over raw AMQP
- **Outbox pattern** — `OrderConfirmed` event written atomically with the order row in one EF Core transaction, background relay delivers to RabbitMQ guaranteeing no lost events
- **At-least-once delivery** with `InboxState` for idempotent consumption
- PostgreSQL database for Order Service with EF Core

### Phase 4 — Observability
- **OpenTelemetry** tracing, metrics, and structured logging across all services
- MassTransit instrumentation — publish and consume spans appear in traces automatically
- gRPC client instrumentation — `CheckStock` calls visible in traces
- **Aspire Dashboard** (standalone Docker) for visualising traces, metrics, and logs
- Structured `ILogger` throughout — no `Console.WriteLine`

### Phase 5 — Resilience with Polly
- **Retry pipeline** — transient gRPC failures retried with exponential backoff
- **Circuit breaker** — opens after consecutive failures, prevents cascade failures to Inventory Service
- **Timeout policy** — gRPC calls capped to prevent slow consumers blocking Order Service
- Pipelines composed in `ResiliencePipelineBuilder` and injected via DI

### Phase 6 — Docker Compose
- Full stack runs with a single `docker compose up --build`
- PostgreSQL with named volume for persistent order data across container restarts
- RabbitMQ with management UI
- Aspire Dashboard wired via OTLP
- Health check dependencies — services wait for RabbitMQ and PostgreSQL to be ready before starting
- Kestrel configured with `0.0.0.0` binding for inter-container communication

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Getting Started

### Run with Docker Compose (recommended)

```bash
git clone https://github.com/siyadoutlook/ShopFlow
cd ShopFlow
docker compose up --build
```

That's it. All services, RabbitMQ, PostgreSQL, and the Aspire Dashboard start together.

| URL | What |
|---|---|
| `http://localhost:5120` | API Gateway (send all requests here) |
| `http://localhost:18888` | Aspire Dashboard — traces, metrics, logs |
| `http://localhost:15672` | RabbitMQ Management UI (guest / guest) |

### Run locally without Docker

Open four terminal windows from the solution root:

```bash
# Terminal 1
dotnet run --project ShopFlow.Gateway

# Terminal 2
dotnet run --project ShopFlow.OrderService

# Terminal 3
dotnet run --project ShopFlow.InventoryService

# Terminal 4
dotnet run --project ShopFlow.NotificationService
```

Start infrastructure separately:

```bash
# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Aspire Dashboard
docker run -d --name aspire-dashboard \
  -p 18888:18888 -p 4317:18889 \
  -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true \
  mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

---

## API Reference

All requests go through the gateway on port `5120`.

### Inventory

```
GET http://localhost:5120/inventory/products
GET http://localhost:5120/inventory/products/{id}
```

### Orders

```
GET  http://localhost:5120/orders
GET  http://localhost:5120/orders/{id}
POST http://localhost:5120/orders
```

**Create order request body:**

```json
{
  "productId": 1,
  "quantity": 2
}
```

**Responses:**
- `201 Created` — order confirmed, stock was available
- `400 Bad Request` — insufficient stock or validation error

---

## Sample Data

Inventory Service starts with three products:

| Id | Name | Price | Stock |
|---|---|---|---|
| 1 | Laptop | £999.99 | 10 |
| 2 | Mouse | £29.99 | 50 |
| 3 | Keyboard | £49.99 | 30 |

---

## What Happens When You Place an Order

```
1. POST /orders hits YARP Gateway
2. Gateway routes to Order Service (REST)
3. Order Service calls Inventory Service via gRPC (Polly: retry + circuit breaker + timeout)
4. If stock insufficient → 400 Bad Request returned immediately
5. If stock available →
     Order saved to PostgreSQL        ┐
     OrderConfirmed written to        ├── single EF Core transaction
     OutboxMessages table             ┘
6. 201 Created returned to client
7. Background relay publishes OrderConfirmed to RabbitMQ
8. Inventory Service consumer → reduces stock
9. Notification Service consumer → logs confirmation email
10. All spans visible in Aspire Dashboard as a single distributed trace
```

---

## Project Structure

```
ShopFlow/
├── docker-compose.yml
├── ShopFlow.Gateway/
│   ├── Program.cs
│   └── appsettings.json
├── ShopFlow.OrderService/
│   ├── Controllers/
│   │   └── OrdersController.cs
│   ├── Data/
│   │   └── OrderDbContext.cs
│   ├── Models/
│   │   ├── Order.cs
│   │   └── OrderStatus.cs
│   └── Program.cs
├── ShopFlow.InventoryService/
│   ├── Controllers/
│   │   └── ProductsController.cs
│   ├── Consumers/
│   │   └── OrderConfirmedConsumer.cs
│   ├── Data/
│   │   └── ProductStore.cs
│   ├── Models/
│   │   └── Product.cs
│   ├── Services/
│   │   └── InventoryGrpcService.cs
│   └── Program.cs
├── ShopFlow.NotificationService/
│   ├── Consumers/
│   │   └── OrderConfirmedConsumer.cs
│   └── Program.cs
└── ShopFlow.Shared/
    ├── Events/
    │   └── OrderConfirmed.cs
    ├── Extensions/
    │   └── ObservabilityExtensions.cs
    └── Protos/
        └── inventory.proto
```

---

## Roadmap

- [x] Phase 1 — YARP API Gateway + REST routing
- [x] Phase 2 — gRPC service-to-service communication
- [x] Phase 3 — MassTransit + RabbitMQ + Outbox pattern
- [x] Phase 4 — OpenTelemetry + Aspire Dashboard
- [x] Phase 5 — Polly resilience pipelines
- [x] Phase 6 — Docker Compose full stack
- [ ] Phase 7 — Kubernetes deployment (AKS)
- [ ] Phase 8 — Authentication and authorisation (JWT at Gateway)
- [ ] Phase 9 — Redis caching (product catalogue in Inventory Service)
- [ ] Phase 10 — MassTransit Saga (order fulfilment with compensation)

---

## Key Lessons

**Why two ports on Inventory Service?** HTTP/1 and HTTP/2 have different handshake requirements. YARP uses HTTP/1 for REST, Order Service uses HTTP/2 for gRPC. Kestrel needs to know upfront which protocol to expect on each port.

**Why the Outbox pattern?** Without it, saving an order and publishing an event are two separate external writes — if the process crashes between them, data becomes inconsistent. The Outbox writes both atomically to one database, guaranteeing the event is never lost.

**Why PostgreSQL over SQLite in Docker?** SQLite stores its file inside the container filesystem. When the container restarts, that file is gone. PostgreSQL with a named Docker volume persists data across restarts.

**Why `0.0.0.0` in Kestrel URLs inside Docker?** Inside a container, `localhost` only refers to that container's loopback interface. Binding to `0.0.0.0` tells Kestrel to accept connections on all network interfaces, including the Docker bridge network that other containers use to reach it.

**Why `RabbitMq__Host` not `RabbitMQ__Host`?** Linux environment variables are case-sensitive. ASP.NET Core normalises `__` to `:` when mapping env vars to configuration keys, but the casing must match exactly between the compose file and the code reading `builder.Configuration["RabbitMq:Host"]`.

**Aspire Dashboard port mapping gotcha** — the dashboard's OTLP gRPC server listens on container port `18889`, not `4317`. The correct Docker mapping is `-p 4317:18889`, not `-p 4317:4317`.
