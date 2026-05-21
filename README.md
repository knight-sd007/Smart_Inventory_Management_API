# Smart Inventory Management API

Production-grade ASP.NET Core Web API for managing inventory, categories, suppliers, and orders. Built with Clean Architecture principles, JWT authentication, and Entity Framework Core.

## Architecture

```
SmartInventory.API              → Presentation Layer (Controllers, Middleware, Extensions)
SmartInventory.Application      → Application Layer (DTOs, Services, Interfaces, Validators)
SmartInventory.Domain           → Domain Layer (Entities, Enums)
SmartInventory.Infrastructure   → Infrastructure Layer (EF Core, Repositories, JWT, Auth)
```

## Security

**WARNING: Never commit secrets to version control.** All sensitive configuration is loaded from environment variables. The `.env` file is gitignored. Use `.env.example` as a template.

## Quick Start

### Prerequisites

- .NET 10 SDK
- SQL Server (local or remote)

### Setup

1. Clone the repository:

```bash
git clone <repo-url>
cd Smart_Inventory_Management_API
```

2. Create your `.env` file from the example:

```bash
cp .env.example .env
```

3. Edit `.env` and fill in real values:

```bash
DB_CONNECTION_STRING=Server=localhost;Database=SmartInventoryDB;User Id=sa;Password=YourRealPassword;Encrypt=true;TrustServerCertificate=false;
JWT_SECRET=<generate-a-cryptographically-secure-random-string-at-least-32-chars>
SEED_ADMIN_PASSWORD=<strong-admin-password>
SEED_DEFAULT_USER_PASSWORD=<strong-default-password>
```

4. Set the environment variables:

```bash
export $(cat .env | xargs)
```

5. Run database migrations and seed:

```bash
dotnet run --project SmartInventory.API
```

The application will auto-apply migrations and seed the database on first run.

### Default Users (after seeding)

| Username | Role    |
| -------- | ------- |
| admin    | Admin   |
| manager  | Manager |
| employee | User    |

Passwords are set via `SEED_ADMIN_PASSWORD` and `SEED_DEFAULT_USER_PASSWORD`.

## Required Environment Variables

| Variable                     | Description                                    | Min Length |
| ---------------------------- | ---------------------------------------------- | ---------- |
| `DB_CONNECTION_STRING`       | SQL Server connection string                   | —          |
| `JWT_SECRET`                 | JWT signing key                                | 32 chars   |
| `JWT_ISSUER`                 | JWT issuer (default: SmartInventoryAPI)        | —          |
| `JWT_AUDIENCE`               | JWT audience (default: SmartInventoryAPIUsers) | —          |
| `JWT_EXPIRY_MINUTES`         | JWT token expiry in minutes (default: 60)      | —          |
| `SEED_ADMIN_PASSWORD`        | Initial admin user password (seed only)        | —          |
| `SEED_DEFAULT_USER_PASSWORD` | Initial user password for seeded accounts      | —          |

## Optional Environment Variables

| Variable                 | Description                               | Default               |
| ------------------------ | ----------------------------------------- | --------------------- |
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development/Production) | Production            |
| `ASPNETCORE_URLS`        | Application URLs                          | http://localhost:5000 |

## API Endpoints

### Authentication

| Method | Endpoint           | Auth | Description      |
| ------ | ------------------ | ---- | ---------------- |
| POST   | /api/auth/register | No   | Register user    |
| POST   | /api/auth/login    | No   | Login, get token |

### Products

| Method | Endpoint                    | Auth | Description          |
| ------ | --------------------------- | ---- | -------------------- |
| GET    | /api/products               | Yes  | List products        |
| GET    | /api/products/{id}          | Yes  | Get product by ID    |
| POST   | /api/products               | Yes  | Create product       |
| PUT    | /api/products/{id}          | Yes  | Update product       |
| DELETE | /api/products/{id}          | Yes  | Delete product       |
| GET    | /api/products/code/{code}   | Yes  | Get product by code  |
| GET    | /api/products/category/{id} | Yes  | Products by category |
| GET    | /api/products/supplier/{id} | Yes  | Products by supplier |
| GET    | /api/products/low-stock     | Yes  | Low stock products   |

### Categories, Suppliers, Orders

Similar CRUD endpoints under `/api/categories`, `/api/suppliers`, `/api/orders`.

## Production Deployment

1. Set all required environment variables in your production environment.
2. Set `ASPNETCORE_ENVIRONMENT=Production`.
3. Use a real SQL Server instance (not LocalDB).
4. Change seed passwords immediately after first deployment.
5. Use a strong, randomly generated JWT secret (e.g., `openssl rand -base64 64`).
6. Enable HTTPS and restrict CORS to specific origins.

### Generate a secure JWT secret

```bash
openssl rand -base64 64
```

## Docker (Coming Soon)

A `docker-compose.yml` with SQL Server + API will be added.

## Security Best Practices

- Rotate `JWT_SECRET` periodically.
- Change seed passwords after initial deployment.
- Use Azure Key Vault / AWS Secrets Manager in production.
- Enable HTTPS in production.
- Restrict CORS to trusted origins.
- Run `dotnet list package --vulnerable` regularly to check for package vulnerabilities.
- Never log sensitive data (passwords, tokens, connection strings).

---

## License

MIT — see [LICENSE](LICENSE) file.

---

## Author

**SOUMOJIT**

- GitHub: [@Soumojit](https://github.com/knight-sd007)
- LinkedIn: [Soumojit](https://linkedin.com/in/soumojit-d-0b8505172)
