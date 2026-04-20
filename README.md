# E-Commerce API (Clean Architecture)

Production-style **ASP.NET Core 8** Web API for an e-commerce backend: **JWT auth**, **Admin/User roles**, **CQRS with MediatR**, **EF Core (SQL Server)**, **FluentValidation**, **AutoMapper**, **Serilog**, **Swagger** with **API versioning**, **tiered rate limiting**, **distributed-cache–ready read caching**, **health checks**, **hardened configuration**, and **global exception handling**.

## Architecture

| Layer | Responsibility |
|--------|------------------|
| **Domain** | Entities, enums, role names; no dependencies on other layers. |
| **Application** | Use cases: commands/queries (MediatR), DTOs, validators, repository & service **interfaces**, AutoMapper profiles, pipeline behaviors (validation + logging). |
| **Infrastructure** | EF Core `DbContext`, migrations, repository + Unit of Work implementations, JWT & BCrypt, HTTP `ICurrentUserService`, database seeding. |
| **API** | Controllers, middleware, Swagger/versioning, DI composition, Serilog host setup. |

Dependencies point **inward**: API → Application + Infrastructure; Infrastructure → Application + Domain; Application → Domain only.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server or **LocalDB** (default connection string uses LocalDB)
- Optional: [local EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) via repo manifest (`.config/dotnet-tools.json`)

## Configuration

Edit `src/ECommerce.API/appsettings.json` (override secrets in production with **environment variables** or a secret store — e.g. `Jwt__Key`, `ConnectionStrings__DefaultConnection`):

- **ConnectionStrings:DefaultConnection** — SQL Server / LocalDB.
- **Jwt** — validated at startup (`ValidateDataAnnotations` + `ValidateOnStart`). **`Key`** must be **≥ 32 characters**.
- **Application:RunMigrationsOnStartup** / **SeedOnStartup** — default **false** in base `appsettings.json` (enterprise default: migrations/seeding via CI/CD or explicit ops). **Development** profile sets both to **true** in `appsettings.Development.json`.
- **Cors:AllowedOrigins** — non-empty array enables strict browser CORS for those origins. If empty and not Development, cross-origin browser calls are **denied** by policy.

Serilog sinks can be extended via `Serilog` section (console is configured by default).

### Enterprise-oriented behavior (summary)

| Area | Approach |
|------|-----------|
| **Secrets & options** | `JwtOptions` uses data annotations; fail fast on bad config. Prefer env/vault for keys in production. |
| **DB resilience** | EF SQL Server: **transient retry** + **split queries** on includes to reduce cartesian explosion. |
| **Read performance** | Product **list** uses **SQL projection** to `ProductDto` (no full aggregate load). Orders **page** loads the graph only for the current page with split queries. |
| **Cache & scale-out** | `ICacheService` over **`IDistributedCache`** (in-memory implementation now; swap to **Redis** for multiple API nodes). Product list keys include a **catalog generation** token so invalidation does not require key enumeration. |
| **Hosting** | **Forwarded headers** for reverse proxies; **HSTS** outside Development; **response compression**; **security headers**; **correlation id** (`X-Correlation-ID`) for logs. |
| **Safety** | **500** responses hide exception detail **outside Development**. **Auth** endpoints use a **stricter rate limit** than the global API. |
| **Observability** | **`/health/live`** (process) and **`/health/ready`** (includes database check). |

## Run the API

```bash
cd src/ECommerce.API
dotnet run
```

- HTTPS profile: check `Properties/launchSettings.json` for the listening URL (e.g. `https://localhost:7xxx`).
- **Swagger UI** (Development): `/swagger` — pick the `v1` document; use **Authorize** with `Bearer <token>` after login.

### Database

When **`Application:RunMigrationsOnStartup`** / **`SeedOnStartup`** are true (default in **Development**), startup runs **`MigrateAsync`** and/or **seed**. In production, prefer running **`dotnet ef database update`** in your pipeline and set these flags to **false** unless you intentionally bootstrap from the app.

**Local EF migrations** (from repo root):

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.API
```

> Use the repo-local `dotnet-ef` (8.0.11) to avoid mismatches with a globally installed tool.

**Seed admin (after first run):**

- Email: `admin@ecommerce.local`
- Password: `Admin123!`

## Tests

```bash
dotnet test
```

Includes sample **unit tests** for `RegisterUserCommandHandler` (Moq + FluentAssertions).

## Features Overview

- **Users** — Admin: paged list, get/update/delete. Registration creates **User** role.
- **Categories / Products** — Public read; Admin CUD. Products support **many-to-many** categories, pagination, **filter/sort** (including price range and category).
- **Cart** — Authenticated **User** role: get/create cart, add/update/remove lines, clear.
- **Orders** — Checkout from cart (stock check, totals); list/detail; Admin can patch **status**.
- **Cross-cutting** — `ExceptionHandlingMiddleware` (problem+json), **global + auth rate limits**, **distributed cache** for product reads, **FluentValidation** via MediatR pipeline, **security / correlation** middleware.

## Sample requests

See `src/ECommerce.API/ECommerce.API.http` for **register**, **login**, **products**, and **checkout** examples.

**Login response (abbreviated):**

```json
{
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-04-21T12:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@ecommerce.local",
    "firstName": "System",
    "lastName": "Administrator",
    "role": "Admin"
  }
}
```

**Paged products:**

```json
{
  "items": [ { "id": "...", "name": "Wireless Headphones", "price": 199.99, "sku": "WH-1000", "categoryNames": [ "Electronics" ] } ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## Folder structure (high level)

```
src/
  ECommerce.Domain/          # Entities, enums, constants
  ECommerce.Application/   # Features (CQRS), DTOs, validators, interfaces
  ECommerce.Infrastructure/# EF Core, repositories, JWT, seeding
  ECommerce.API/           # Host, controllers, middleware, Swagger
tests/
  ECommerce.Application.Tests/
.config/
  dotnet-tools.json        # dotnet-ef 8.0.11
```

## Security notes

- Passwords: **BCrypt** (work factor 12) via `IPasswordHasher`.
- JWT: validated issuer, audience, signing key, lifetime; roles in claims for `[Authorize(Roles = ...)]`.
- Replace default **Jwt:Key** and seed credentials before any public deployment.

## License

Sample project for learning and extension; adapt licensing as needed for your product.
