# Smart Inventory Management API

Production-grade ASP.NET Core Web API for managing inventory, categories, suppliers, and orders. Built with Clean Architecture principles, JWT authentication, Entity Framework Core, and xUnit test suite targeting .NET 10.

---

## Architecture

```
SmartInventory.API              → Presentation Layer (Controllers, Middleware, Extensions)
SmartInventory.Application      → Application Layer (DTOs, Business Logic Services, Interfaces)
SmartInventory.Domain           → Domain Layer (Entities, Enums)
SmartInventory.Infrastructure   → Infrastructure Layer (EF Core, Repositories, JWT, Auth, Seed)
tests/SmartInventory.Tests      → Unit & Integration Test Suite (xUnit, Moq, EF Core InMemory)
```

---

## Security Guidelines

> [!WARNING]
> **Never commit secrets or credentials to version control.** All sensitive configurations are dynamically loaded from environment variables using a zero-dependency `.env` loader. The `.env` file is gitignored. Use `.env.example` as a template.

---

## Quick Start

### Prerequisites

- .NET 10 SDK
- SQL Server (local, remote, or SQL Server Express)

### Local Setup

1. Clone the repository:

```bash
git clone git@github.com:knight-sd007/Smart_Inventory_Management_API.git
cd Smart_Inventory_Management_API
```

2. Create your `.env` file from `.env.example`:

```bash
cp .env.example .env
```

3. Edit `.env` with real local development values:

```env
DB_CONNECTION_STRING=Server=localhost;Database=SmartInventoryDB;User Id=sa;Password=YourStrongPassword123!;Encrypt=true;TrustServerCertificate=true;
JWT_SECRET=a_super_secret_cryptographic_key_that_is_at_least_32_characters_long
SEED_ADMIN_PASSWORD=StrongAdminPassword123!
SEED_DEFAULT_USER_PASSWORD=StrongUserPassword123!
```

4. Build the solution:

```bash
dotnet build SmartInventoryAPI.slnx
```

5. Run the API:

```bash
dotnet run --project SmartInventory.API
```

---

## Running Unit & Integration Tests

The solution includes an xUnit test suite (`SmartInventory.Tests`) verifying business services, JWT authentication, SHA256 password hashing, and repository CRUD operations via EF Core InMemory database.

To execute the test suite:

```bash
dotnet test SmartInventoryAPI.slnx
```

---

## Default Seeded Accounts

| Username | Role | Description |
|---|---|---|
| `admin` | Admin | Full system administrative access |
| `manager` | Manager | Inventory and order management access |
| `employee` | Staff | View and product lookup access |

*Passwords are configured via `SEED_ADMIN_PASSWORD` and `SEED_DEFAULT_USER_PASSWORD` environment variables.*

---

## Required Environment Variables

| Variable | Description | Min Length / Default |
|---|---|---|
| `DB_CONNECTION_STRING` | SQL Server connection string | Required |
| `JWT_SECRET` | Cryptographic JWT signing key | Min 32 characters |
| `JWT_ISSUER` | JWT token issuer claim | `SmartInventoryAPI` |
| `JWT_AUDIENCE` | JWT token audience claim | `SmartInventoryAPIUsers` |
| `JWT_EXPIRY_MINUTES` | Token expiry duration | `60` |
| `SEED_ADMIN_PASSWORD` | Initial admin user password | Required for seed |
| `SEED_DEFAULT_USER_PASSWORD` | Initial default user password | Required for seed |

---

## Core API Endpoints

### Authentication
- `POST /api/auth/register` — Register a new user
- `POST /api/auth/login` — Authenticate user and receive JWT bearer token

### Products
- `GET /api/products` — List paginated products
- `GET /api/products/{id}` — Get product details by ID
- `GET /api/products/code/{code}` — Lookup product by code
- `GET /api/products/low-stock` — Query low stock products below reorder level
- `POST /api/products` — Create new product (Manager/Admin)
- `PUT /api/products/{id}` — Update product details (Manager/Admin)
- `DELETE /api/products/{id}` — Soft delete product (Admin)

### Categories, Suppliers, & Orders
Similar CRUD endpoints managed under `/api/categories`, `/api/suppliers`, and `/api/orders`.

---

## License

MIT — see [LICENSE](LICENSE) file.

---

## Author

**SOUMOJIT**

- GitHub: [@knight-sd007](https://github.com/knight-sd007)
- LinkedIn: [Soumojit](https://linkedin.com/in/soumojit-d-0b8505172)
