# Eshop

## Project Overview

Eshop is a simple .NET 8 e-commerce web application. Users can browse a product catalogue and manage a personal shopping cart through a two-page Razor Pages UI. The backend exposes a JSON REST API secured with JWT Bearer authentication, backed by SQL Server via Entity Framework Core and ASP.NET Core Identity.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core |
| ORM | EF Core (SQL Server) |
| Identity | ASP.NET Core Identity — long (`long`) primary keys |
| Auth | JWT Bearer authentication |
| UI | Razor Pages (two pages) |

---

## Architecture

The solution uses **Onion / Clean Architecture**. Dependencies flow inward — outer layers depend on inner layers; inner layers have no knowledge of outer layers.

```
Eshop/
└── src/
    ├── Shop.Domain/          # Entities (Product, Cart, CartItem) and BaseEntity
    ├── Shop.Application/     # DTOs, request models, service interfaces (IProductService, ICartService)
    ├── Shop.Infrastructure/  # EF Core AppDbContext, Identity, service implementations, DI registration
    └── Shop.Web/             # API controllers + Razor Pages UI
```

**Dependency direction:** `Shop.Web → Shop.Application → Shop.Domain`
`Shop.Infrastructure` implements the abstractions defined in `Shop.Application` and depends on both `Shop.Application` and `Shop.Domain`.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or a full instance)
- *(Optional)* EF Core CLI tools — `dotnet tool install --global dotnet-ef`

---

## Configuration

Edit `src/Shop.Web/appsettings.json` (or use user-secrets / environment variables):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShopDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Issuer": "Shop.Web",
    "Audience": "Shop.Web",
    "Key": "YourSuperSecretKeyHere12345678901234567890",
    "ExpMinutes": 60
  }
}
```

> **Note:** `Jwt:Key` above is a development / assessment key only. Replace it with a strong secret before any production deployment.

---

## Database Setup / Migrations

### Package Manager Console (Visual Studio)

```powershell
# Add a migration
Add-Migration <MigrationName> -Project Shop.Infrastructure -StartupProject Shop.Web -Context AppDbContext

# Apply migrations
Update-Database -Project Shop.Infrastructure -StartupProject Shop.Web -Context AppDbContext
```

### .NET CLI

```bash
# Add a migration
dotnet ef migrations add <MigrationName> --project src/Shop.Infrastructure --startup-project src/Shop.Web

# Apply migrations
dotnet ef database update --project src/Shop.Infrastructure --startup-project src/Shop.Web
```

> **Auto-apply on startup:** `DbInitializer.InitializeAsync` is called from `Program.cs` at startup. It runs `MigrateAsync()` automatically and seeds default data if the tables are empty — no manual migration step is required for a first run.

---

## Seeded Data

### Products

Ten sample products (Wireless Headphones, Mechanical Keyboard, USB-C Hub, etc.) are inserted on first run when the `Products` table is empty.

### Identity Users

| Email | Password | Notes |
|---|---|---|
| `admin@shop.com` | `P@ssw0rd!` | Admin seed account |
| `user@shop.com` | `P@ssw0rd!` | Regular user seed account |

Both accounts have `EmailConfirmed = true` and are created on first run if they do not already exist.

---

## Running the App

```bash
dotnet run --project src/Shop.Web
```

| Path | Description |
|---|---|
| `/login` | Login page |
| `/` | Main page — product catalogue + cart |

---

## API Endpoints

All endpoints are under the base path served by `Shop.Web`.

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a new account |
| `POST` | `/api/auth/login` | Public | Login and receive a JWT token |

### Products

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/products` | Public | List all products |

### Cart

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/cart` | **Required** | Get (or create) the current user's cart |
| `POST` | `/api/cart/items` | **Required** | Add a product to the cart |
| `PUT` | `/api/cart/items/{itemId}` | **Required** | Update quantity of a cart item (set to 0 to remove) |
| `DELETE` | `/api/cart/items/{itemId}` | **Required** | Remove a cart item |

**Token usage in the UI:** The JWT is stored in `localStorage` under the key `auth_token`. Authenticated requests include the header:

```
Authorization: Bearer <token>
```

---

## Frontend Notes

- **Two pages only:** `/login` (Login) and `/` (Main).
- The **Main page** displays the product catalogue and the user's cart. Products can be added to the cart, and cart items can have their quantity updated or be removed.
- **Logout** clears the `auth_token` from `localStorage` and redirects to `/login`.
- **401 handling:** If any authenticated API call returns `401 Unauthorized`, `handleUnauthorized()` in `app.js` automatically clears the token and redirects to `/login`.

---

## Validation / Error Handling

- `POST /api/cart/items` — quantity must be between **1 and 999**; returns `400` otherwise.
- `PUT /api/cart/items/{itemId}` — quantity must be between **0 and 999** (0 removes the item); returns `400` otherwise.
- Cart endpoints return `404` when the referenced cart item does not exist.
- Product existence is validated before adding to the cart (`404` if not found).
- Unauthenticated requests to cart endpoints return `401 Unauthorized`.
- Login failures return `401 Unauthorized`.
- Registration failures (e.g. weak password, duplicate email) return `400 Bad Request` with error details.

---

## Developer Notes

- **Identity primary keys:** ASP.NET Core Identity is configured with `long` primary keys (`IdentityDbContext<ApplicationUser, IdentityRole<long>, long>`).
- **BaseEntity:** `Shop.Domain/Common/BaseEntity.cs` defines a `long Id` primary key inherited by `Product`, `Cart`, and `CartItem`.
- **Token expiry:** JWT expiration is driven by `Jwt:ExpMinutes` in `appsettings.json`. `ClockSkew` is set to `TimeSpan.Zero` so tokens expire exactly at the configured time.
- **DI registration:** All infrastructure services (DbContext, Identity, JWT auth, `IProductService`, `ICartService`) are wired up in `Shop.Infrastructure/DependencyInjection.cs` via the `AddInfrastructure` extension method.
