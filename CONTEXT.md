# Smart Inventory Management API - Project Plan

## Current State Analysis
The project has a partially established Clean Architecture:
- `SmartInventory.Domain`: Contains Entities (`Product`, `Category`, `Supplier`, `Order`, `OrderItem`, `User`) and Enums.
- `SmartInventory.Application`: Contains DTOs, Interfaces, Mappings, Services, and Validators.
- `SmartInventory.Infrastructure`: Contains `AppDbContext`, Repositories, and basic services (AuthService, JwtService).
- `SmartInventory.API`: Basic default ASP.NET Core Web API template. `Program.cs` needs to be fully wired up.

## To-Do Tasks

### Phase 1: Configuration & Wiring (`SmartInventory.API`)
1.  **Database Configuration**:
    - Update `Program.cs` to register `AppDbContext`.
    - Retrieve Connection String from an Environment Variable (`DB_CONNECTION_STRING`).
2.  **Authentication Configuration**:
    - Register JWT Authentication in `Program.cs`.
    - Retrieve JWT Secret, Issuer, and Audience from Environment Variables (`JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`).
3.  **Dependency Injection**:
    - Register all Application Services and Infrastructure Repositories in `Program.cs` (or via the existing `ServiceExtensions.cs` / `DependencyInjectionExtensions.cs`).
4.  **Middleware**:
    - Register the `ExceptionMiddleware` in the pipeline.
5.  **Swagger/OpenAPI**:
    - Configure Swagger generation in `Program.cs` including JWT Bearer Authentication support.

### Phase 2: API Controllers
Create the following controllers in `SmartInventory.API/Controllers`, implementing complete CRUD, pagination, and role-based authorization where applicable:
1.  `AuthController` (Login, Register)
2.  `CategoriesController`
3.  `SuppliersController`
4.  `ProductsController`
5.  `OrdersController`

### Phase 3: Database Migrations & Seeding
1.  Add Data Seeding logic to initialize the database with:
    - Default Admin user.
    - Sample Categories, Suppliers, and Products.
2.  Create the initial EF Core Migration.
3.  Apply migrations on startup automatically (optional but good for Docker).

### Phase 4: Dockerization
1.  Create a multi-stage `Dockerfile` in the root (or API folder) for the ASP.NET Core API.
2.  Create a `docker-compose.yml` file to orchestrate the SQL Server database and the API container.
    - Setup environment variables mapping in `docker-compose.yml`.

### Phase 5: Documentation & Polish
1.  **README.md**: Create a highly professional, Fiverr-ready README.
    - Project description & Architecture.
    - Setup instructions (Docker & Local).
    - Environment Variables list.
    - API Endpoints summary.
    - Screenshots section (placeholders for DB and Swagger).
2.  **Clean up**: Remove default `WeatherForecast` code from `Program.cs` and API.

## Notes on Environment Variables
The application must securely use environment variables for sensitive data.
- `DB_CONNECTION_STRING`
- `JWT_SECRET`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
