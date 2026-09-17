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

- .NET 10 SDK (for local development)
- Docker & Docker Compose (for containerized execution)
- PostgreSQL 16+ (local, remote, or containerized via Docker Compose)

### Running with Docker Compose (Recommended)

1. Clone the repository and navigate to the project directory:

```bash
git clone git@github.com:knight-sd007/Smart_Inventory_Management_API.git
cd Smart_Inventory_Management_API
```

2. Create your `.env` file from `.env.example`:

```bash
cp .env.example .env
```

3. Edit `.env` with your secure credentials:

```env
POSTGRES_DB=SmartInventoryDB
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_postgres_password_here
DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=SmartInventoryDB;Username=postgres;Password=your_secure_postgres_password_here
JWT_SECRET=your_secure_random_jwt_secret_minimum_32_characters_here
SEED_ADMIN_PASSWORD=your_secure_admin_seed_password_here
SEED_DEFAULT_USER_PASSWORD=your_secure_default_user_password_here
```

4. Build and start the container stack:

```bash
docker compose up -d --build
```

5. Services will be available at:
- **API Endpoint**: `http://127.0.0.1:5003`
- **Swagger Documentation**: `http://127.0.0.1:5003/swagger`
- **Health Check Probe**: `http://127.0.0.1:5003/health`
- **PostgreSQL Database**: Internal only on compose network (port 5432, named volume `smartinventory-pgdata`)

6. Stop the container stack:

```bash
docker compose down
```

### Local Setup (Without Docker)

1. Ensure a PostgreSQL 16+ database is running locally.
2. Configure `DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=SmartInventoryDB;Username=postgres;Password=your_secure_postgres_password_here` in `.env`.
3. Build the solution:

```bash
dotnet build SmartInventoryAPI.slnx
```

4. Run the API:

```bash
dotnet run --project SmartInventory.API
```

---

## Running Unit & Integration Tests

The solution includes an xUnit test suite (`SmartInventory.Tests`) verifying business services, JWT authentication, BCrypt password hashing, and repository CRUD operations via EF Core InMemory database.

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
| `POSTGRES_DB` | PostgreSQL database name | `SmartInventoryDB` |
| `POSTGRES_USER` | PostgreSQL superuser username | `postgres` |
| `POSTGRES_PASSWORD` | PostgreSQL superuser password | Required in Docker Compose |
| `DB_CONNECTION_STRING` | PostgreSQL connection string | Required |
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

## Security & Authentication

- **Password Cryptography:** Password authentication uses standard BCrypt adaptive work-factor key derivation (`BCrypt.Net-Next`) with salted hashing for secure credential storage.

---

## License

MIT — see [LICENSE](LICENSE) file.

---

## Author

**SOUMOJIT**

- GitHub: [@knight-sd007](https://github.com/knight-sd007)
- LinkedIn: [Soumojit](https://linkedin.com/in/soumojit-d-0b8505172)
