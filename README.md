# Skately — Backend API

The Skately backend is a RESTful API built with ASP.NET Core 8. It powers an e-commerce platform with product catalog management, Redis-backed shopping carts, Stripe payment processing, order fulfillment, and role-based administration.

The solution follows a **Clean Architecture** layout with separate projects for domain logic, infrastructure, and the HTTP API layer.

---

## Features

### Product Catalog
- Paginated product listing with filtering, sorting, and search (specification pattern)
- Brand and type filter endpoints
- Full CRUD for products (Admin role)

### Shopping Cart
- Redis-backed cart storage with create, read, update, and delete operations
- Cart items persist independently of user authentication

### Orders
- Order creation linked to Stripe Payment Intents
- User-scoped order history and order detail retrieval
- Delivery method selection and shipping address capture
- Order status lifecycle: Pending → PaymentReceived → Refunded / PaymentFailed

### Payments (Stripe)
- Payment Intent creation and updates tied to cart totals
- Stripe webhook handler for `payment_intent.succeeded`, `payment_intent.payment_failed`, `payment_intent.canceled`, and `charge.refunded`
- Admin-initiated refunds via the Stripe API

### Authentication & Authorization
- ASP.NET Core Identity with cookie-based auth (`MapIdentityApi`)
- Custom account endpoints: register, logout, user info, auth status, address management
- Role-based access: **Admin** role for product management and order administration

### Real-time Notifications
- SignalR hub at `/hub/notifications`
- Pushes `OrderCompleteNotification` to buyers when payment succeeds

### Data & Seeding
- EF Core migrations applied automatically on startup
- Seed data: admin user, products (JSON), and delivery methods (JSON)

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 (SQL Server) |
| Database | SQL Server / LocalDB |
| Cache | Redis (StackExchange.Redis) |
| Auth | ASP.NET Core Identity |
| Payments | Stripe.net |
| Real-time | SignalR |
| Architecture | Clean Architecture (Core → Infrastructure → API) |

---

## Solution Structure

```
Backend/
├── Skinet.sln
├── API/                    # HTTP layer — controllers, middleware, SignalR, DTOs
├── Core/                   # Domain entities, interfaces, specifications
├── Infrastructure/         # EF Core context, repositories, Redis cart, Stripe service
└── docker-compose.yml      # Redis container definition
```

---

## Prerequisites

- **.NET 8 SDK**
- **SQL Server** or **SQL Server LocalDB** (default connection uses LocalDB)
- **Redis** 7+ (local install or Docker)
- **Stripe account** (test keys for development)
- **SSL certificates** for HTTPS development (`cert.pem`, `cert-key.pem` in the `API` project root)

---

## Getting Started

### 1. Start Redis

Using Docker Compose (recommended):

```bash
cd Backend
docker compose up -d
```

This starts Redis on port **6379**, matching the default connection string.

### 2. Configure application settings

Edit `API/appsettings.json` (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for sensitive values):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Skinet;Trusted_Connection=True;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  },
  "StripeSettings": {
    "PublishableKey": "<your-stripe-publishable-key>",
    "SecretKey": "<your-stripe-secret-key>",
    "whSecret": "<your-stripe-webhook-signing-secret>"
  }
}
```

> **Security:** Do not commit real Stripe secret keys or webhook secrets to source control. Use User Secrets or environment variables in production.

### 3. Configure SSL certificates

Place development certificates in the `API` project directory:

```
API/
├── cert.pem
└── cert-key.pem
```

The `https` launch profile in `Properties/launchSettings.json` references these files.

### 4. Run the API

```bash
cd Backend/API
dotnet run --launch-profile https
```

The API starts at:
- **HTTPS:** `https://localhost:5001`
- **HTTP:** `http://localhost:5000`

On first run, EF Core applies pending migrations and seeds the database with products, delivery methods, and a default admin user.

---

## API Endpoints

All controllers are routed under `api/[controller]`.

| Controller | Key Endpoints | Auth |
|------------|---------------|------|
| **Products** | `GET /api/products`, `GET /api/products/{id}`, `GET /api/products/brands`, `GET /api/products/types` | Public |
| **Products** | `POST`, `PUT`, `DELETE /api/products/{id}` | Admin |
| **Cart** | `GET`, `POST`, `DELETE /api/cart` | Public |
| **Orders** | `POST /api/order`, `GET /api/order`, `GET /api/order/{id}` | Authenticated |
| **Payments** | `POST /api/payments/{cartId}`, `GET /api/payments/delivery-methods` | Mixed |
| **Payments** | `POST /api/payments/webhook` | Anonymous (Stripe signature) |
| **Account** | `POST /api/account/register`, `GET /api/account/user-info`, `POST /api/account/logout` | Mixed |
| **Admin** | `GET /api/admin/orders`, `GET /api/admin/order/{id}`, `POST /api/admin/orders/refund/{id}` | Admin |
| **Identity** | `/api/register`, `/api/login`, `/api/manage/info`, etc. | Mixed |

### SignalR

| Hub | Path | Auth |
|-----|------|------|
| NotificationHub | `/hub/notifications` | Authenticated |

---

## Default Seed Data

| Resource | Details |
|----------|---------|
| **Admin user** | `admin@admin.com` / `Admin@123` (Admin role) |
| **Products** | Loaded from `Infrastructure/Data/SeedData/products.json` |
| **Delivery methods** | Loaded from `Infrastructure/Data/SeedData/delivery.json` |

---

## Stripe Webhook Setup (Local Development)

For payment status updates to flow correctly, Stripe webhooks must reach the API:

1. Install the [Stripe CLI](https://stripe.com/docs/stripe-cli)
2. Forward events to the local endpoint:

```bash
stripe listen --forward-to https://localhost:5001/api/payments/webhook
```

3. Copy the webhook signing secret (`whsec_...`) into `StripeSettings:whSecret` in your configuration.

---

## CORS

The API allows credentialed requests from:

- `http://localhost:4200`
- `https://localhost:4200`

Update the CORS policy in `Program.cs` if your frontend runs on a different origin.

---

## Database Migrations

Migrations run automatically on startup via `context.Database.MigrateAsync()`. To create a new migration manually:

```bash
cd Backend/API
dotnet ef migrations add <MigrationName> --project ../Infrastructure
```

---

## Docker

`docker-compose.yml` provides a Redis service. SQL Server via Docker is commented out but can be enabled by uncommenting the `sql` service block and updating the connection string.

---

## Error Handling

A global `ExceptionMiddleware` catches unhandled exceptions and returns consistent error responses. A `BuggyController` is available for testing error scenarios during development.

---

## Related

- [Frontend README](https://github.com/mostafaX404/SkatelyFrontEnd) — Angular client setup and configuration
