# Developer Evaluation - Sales API

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-blue)](https://www.postgresql.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-8.0-green)](https://www.mongodb.com/)
[![Redis](https://img.shields.io/badge/Redis-7.4-red)](https://redis.io/)

A comprehensive **Sales Management API** built with **.NET 8** implementing **Clean Architecture**, **CQRS pattern**, and **Domain-Driven Design (DDD)** principles. This project features a complete sales system with automatic discount calculations, product catalog management, shopping carts, and role-based access control.

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Seed Data](#seed-data)
- [API Documentation](#api-documentation)
- [Business Rules](#business-rules)
- [Testing](#testing)
- [Security](#security)

## Overview

This project serves as a **senior developer evaluation**, demonstrating proficiency in:

- **Clean Architecture** with proper layer separation
- **CQRS** (Command Query Responsibility Segregation) via MediatR
- **Domain-Driven Design** with rich domain entities and business rules
- **RESTful API** design with comprehensive CRUD operations
- **Multi-database** architecture (PostgreSQL + MongoDB + Redis)
- **JWT authentication** with role-based authorization
- **Automated testing** across unit, integration, and functional layers

### Core Business Domain

The API manages a **Sales System** for a product catalog, handling:

- **Sales Management**: Complete CRUD operations with automatic discount calculation
- **Product Catalog**: Beverages, snacks, and energy drinks inventory
- **Shopping Carts**: Customer cart management with real-time price calculation
- **Branch Management**: Multi-branch sales tracking
- **User Management**: Role-based access control (Admin, Manager, Customer)
- **Discount Engine**: Automatic discounts based on quantity tiers (4–9 items → 10%, 10–20 items → 20%)

## Tech Stack

### Backend
- **[.NET 8.0](https://dotnet.microsoft.com/)** — Web framework
- **[ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)** — Web API
- **[C# 12](https://docs.microsoft.com/en-us/dotnet/csharp/)** — Language

### Architecture & Patterns
- **[MediatR 12](https://github.com/jbogard/MediatR)** — CQRS and mediator pattern
- **[AutoMapper](https://github.com/AutoMapper/AutoMapper)** — Object mapping
- **[FluentValidation](https://fluentvalidation.net/)** — Input validation
- **[Entity Framework Core 8](https://docs.microsoft.com/en-us/ef/core/)** — ORM for PostgreSQL

### Databases
- **[PostgreSQL 13](https://www.postgresql.org/)** — Primary write database (port `5433`)
- **[MongoDB 8.0](https://www.mongodb.com/)** — Document store / read models (port `27017`)
- **[Redis 7.4](https://redis.io/)** — In-memory cache (port `6380`)

### Observability & Documentation
- **[Serilog](https://serilog.net/)** — Structured logging
- **[Swagger/OpenAPI](https://swagger.io/)** — Interactive API docs (Swashbuckle 6.8)
- **Health Checks** — `/health` endpoint for all dependencies

### Testing
- **[xUnit](https://xunit.net/)** — Test framework
- **[NSubstitute](https://nsubstitute.github.io/)** — Mocking
- **[Bogus](https://github.com/bchavez/Bogus)** — Realistic test data generation

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                      │
│           WebApi / Controllers / Middleware                 │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                       │
│           Commands / Queries / Handlers / Validators        │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                          │
│           Entities / Business Rules / Domain Events         │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                      │
│       Repositories / EF Core / MongoDB / Redis / IoC        │
└─────────────────────────────────────────────────────────────┘
```

### Project Structure

```
developer-evaluation/
├── src/
│   ├── DeveloperEvaluation.WebApi/       # ASP.NET Core API, controllers, Swagger
│   ├── DeveloperEvaluation.Application/  # CQRS commands, queries, handlers
│   ├── DeveloperEvaluation.Domain/       # Entities, value objects, domain events
│   ├── DeveloperEvaluation.ORM/          # EF Core DbContext, migrations, seeding
│   ├── DeveloperEvaluation.IoC/          # Dependency injection registration
│   └── DeveloperEvaluation.Common/       # Shared utilities, JWT, health checks
├── tests/
│   ├── DeveloperEvaluation.Unit/         # Unit tests
│   ├── DeveloperEvaluation.Integration/  # Integration tests
│   └── DeveloperEvaluation.Functional/   # End-to-end API tests
├── docker-compose.yml                    # Full stack (API + databases)
├── docker-compose.infra.yml             # Infrastructure only (databases)
├── DeveloperEvaluation.sln
└── coverage-report.sh
```

## Getting Started

### Prerequisites

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)**
- **[Git](https://git-scm.com/)**

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/LucasGeek/developer-evaluation.git
   cd developer-evaluation
   ```

2. **Start the database containers**
   ```bash
   docker-compose -f docker-compose.infra.yml up -d
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Configure the application**

   Create `src/DeveloperEvaluation.WebApi/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5433;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"
     },
     "MongoDB": {
       "ConnectionString": "mongodb://developer:ev@luAt10n@localhost:27017",
       "DatabaseName": "developer_evaluation"
     },
     "Redis": {
       "ConnectionString": "localhost:6380,password=ev@luAt10n"
     },
     "Jwt": {
       "SecretKey": "your-secret-key-min-32-chars",
       "Issuer": "DeveloperEvaluation",
       "Audience": "DeveloperEvaluation"
     }
   }
   ```

5. **Apply database migrations**
   ```bash
   dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
   ```

6. **Seed initial data** *(optional but recommended)*
   ```bash
   dotnet run --project src/DeveloperEvaluation.WebApi -- --seed
   ```

7. **Start the application**
   ```bash
   dotnet run --project src/DeveloperEvaluation.WebApi
   ```

8. **Access the API**
   - **Swagger UI**: http://localhost:5119/swagger
   - **API Base**: http://localhost:5119/api
   - **Health Check**: http://localhost:5119/health
   - **HTTPS**: https://localhost:7181/swagger

## Database Setup

### Docker Compose

Two compose files are provided:

| File | Purpose |
|------|---------|
| `docker-compose.infra.yml` | Infrastructure only (PostgreSQL, MongoDB, Redis) |
| `docker-compose.yml` | Full stack including the WebAPI container |

```bash
# Start infrastructure only (recommended for local development)
docker-compose -f docker-compose.infra.yml up -d

# Start full stack with Docker
docker-compose up -d

# View service status
docker-compose -f docker-compose.infra.yml ps

# Stop all services
docker-compose -f docker-compose.infra.yml down
```

### Port Mapping

| Service    | Container Port | Host Port |
|-----------|---------------|-----------|
| PostgreSQL | 5432          | **5433**  |
| MongoDB    | 27017         | **27017** |
| Redis      | 6379          | **6380**  |

### Entity Framework Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi

# Apply migrations
dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi

# Drop database (WARNING: destroys all data)
dotnet ef database drop -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
```

## Seed Data

The application ships with a `DataSeeder` that populates the database with realistic test data. Seeding runs as a **separate one-off command** to keep normal startup fast.

### Running Seed

```bash
# Apply migrations + seed in one go (seeder calls MigrateAsync internally)
dotnet run --project src/DeveloperEvaluation.WebApi -- --seed

# Force full re-seed (drop → migrate → seed)
dotnet ef database drop -f -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
dotnet run --project src/DeveloperEvaluation.WebApi -- --seed
```

> **Note**: Seeding is idempotent — it checks whether data already exists before inserting. Running it twice is safe.

### What Gets Seeded

#### Users (5 total)

| Role     | Email                            | Password      |
|----------|----------------------------------|---------------|
| Admin    | `admin@developereval.com`        | `Admin123!`   |
| Manager  | `manager@developereval.com`      | `Manager123!` |
| Customer | `customer1@developereval.com`    | `Customer123!`|
| Customer | `customer2@developereval.com`    | `Customer123!`|
| Customer | `customer3@developereval.com`    | `Customer123!`|

#### Products (17 total)

| Category           | Products                                         |
|--------------------|--------------------------------------------------|
| Beverages          | Budweiser, Stella Artois, Corona, Brahma, Antarctica Pilsen |
| Premium Beverages  | Beck's, Leffe, Hoegaarden                        |
| Snacks             | Doritos, Lay's, Pringles, Cheetos                |
| Soft Drinks        | Guaraná Antarctica, Pepsi, H2OH                  |
| Energy Drinks      | Red Bull, Monster Energy                         |

Each product has realistic pricing ($2.99–$15.99), random ratings (3.5–5.0 stars), and review counts (100–2,500).

#### Shopping Carts & Sales
- **3 carts** — one per customer, each with 2–5 random products
- **15 sales** — spread across 5 Brazilian branch locations, with discounts applied and ~15% randomly cancelled

## API Documentation

### Swagger UI

Interactive documentation is available at **http://localhost:5119/swagger** (Development mode only).

Features: request/response examples, JWT authentication, schema browser, request duration display.

### Authentication

```bash
# 1. Get a JWT token
curl -X POST http://localhost:5119/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@developereval.com","password":"Admin123!"}'

# 2. Use the token
curl http://localhost:5119/api/products \
  -H "Authorization: Bearer <token>"
```

In Swagger UI: click **Authorize**, then enter `Bearer <token>`.

### Endpoints

#### Authentication
```
POST   /api/auth/login          # Authenticate and get JWT token
```

#### Users
```
GET    /api/users               # List users (Admin/Manager)
POST   /api/users               # Create user (Admin)
GET    /api/users/{id}          # Get user by ID
PUT    /api/users/{id}          # Update user
DELETE /api/users/{id}          # Delete user (Admin)
```

#### Products
```
GET    /api/products                      # List with pagination and filters
POST   /api/products                      # Create product (Manager/Admin)
GET    /api/products/{id}                 # Get product by ID
PUT    /api/products/{id}                 # Update product (Manager/Admin)
DELETE /api/products/{id}                 # Delete product (Manager/Admin)
GET    /api/products/category/{category}  # Filter by category
```

#### Shopping Carts
```
GET    /api/carts               # List carts (Manager/Admin)
POST   /api/carts               # Create cart
GET    /api/carts/{id}          # Get cart by ID
PUT    /api/carts/{id}          # Update cart
DELETE /api/carts/{id}          # Delete cart
```

#### Branches
```
GET    /api/branches            # List branches
POST   /api/branches            # Create branch (Admin)
GET    /api/branches/{id}       # Get branch by ID
PUT    /api/branches/{id}       # Update branch (Admin)
DELETE /api/branches/{id}       # Delete branch (Admin)
```

#### Sales
```
GET    /api/sales               # List sales
POST   /api/sales               # Create sale
GET    /api/sales/{id}          # Get sale by ID
PUT    /api/sales/{id}          # Update sale
DELETE /api/sales/{id}          # Cancel sale
```

#### Health
```
GET    /health                  # Health check (all dependencies)
```

### Query Parameters

All list endpoints support:

- **Pagination**: `?page=1&size=20`
- **Sorting**: `?sort=price desc,title asc`
- **Filtering**: e.g. `?category=beverages&minPrice=5&maxPrice=20`

### Request/Response Examples

#### Create Sale
```json
POST /api/sales
{
  "saleNumber": "SALE-2024-001",
  "saleDate": "2024-01-15T10:30:00Z",
  "customerId": "<guid>",
  "customerDescription": "John Doe",
  "branchId": "<guid>",
  "branchDescription": "São Paulo - Centro",
  "items": [
    {
      "productId": "<guid>",
      "productDescription": "Budweiser Lager Beer",
      "quantity": 12,
      "unitPrice": 8.99
    }
  ]
}
```

#### Response (201 Created)
```json
{
  "success": true,
  "data": {
    "id": "<guid>",
    "saleNumber": "SALE-2024-001",
    "date": "2024-01-15T10:30:00Z",
    "customerDescription": "John Doe",
    "branchDescription": "São Paulo - Centro",
    "totalAmount": 86.30,
    "cancelled": false,
    "items": [
      {
        "productDescription": "Budweiser Lager Beer",
        "quantity": 12,
        "unitPrice": 8.99,
        "discount": 21.58,
        "total": 86.30
      }
    ]
  }
}
```

## Business Rules

### Discount Calculation

Discounts are applied automatically per item when the sale is created:

| Quantity     | Discount         |
|-------------|-----------------|
| 1–3 items   | 0%              |
| 4–9 items   | **10%**         |
| 10–20 items | **20%**         |
| 21+ items   | Not allowed     |

```csharp
// Example: 12 units of Budweiser at $8.99
var item = new SaleItem(saleId, productId, "Budweiser", 12, 8.99m);
item.ApplyDiscount();
// Discount: 20% → $21.58
// Total:    $86.30
```

### Validation Rules

- **Maximum 20 items** per product per sale line
- **Positive quantities** required (minimum 1)
- **Unique email addresses** per user
- **Strong passwords**: 8+ chars, uppercase, lowercase, digit, special character

## Testing

### Structure

```
tests/
├── DeveloperEvaluation.Unit/        # Domain logic, validators, CQRS handlers
├── DeveloperEvaluation.Integration/ # Database and repository tests
└── DeveloperEvaluation.Functional/  # End-to-end API tests
```

### Running Tests

```bash
# Run all tests
dotnet test DeveloperEvaluation.sln

# Run a specific project
dotnet test tests/DeveloperEvaluation.Unit/

# Run with verbose output
dotnet test --verbosity normal

# Run with code coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

### Coverage Report

A convenience script generates an HTML coverage report:

```bash
chmod +x coverage-report.sh
./coverage-report.sh
# Opens: TestResults/CoverageReport/index.html
```

## Security

- **JWT Bearer tokens** — configurable expiration via `appsettings`
- **Role-based authorization** — Admin, Manager, Customer roles enforced per endpoint
- **Password hashing** — secure one-way hashing (no plaintext storage)
- **FluentValidation** — all inputs validated before handler execution
- **EF Core parameterization** — SQL injection protection by default
- **`appsettings.Development.json` excluded from git** — never commit credentials

---

## Contact

- **Author**: Lucas Albuquerque
- **Email**: lucas.albuquerque.gk@gmail.com
- **Repository**: https://github.com/LucasGeek/developer-evaluation
- **Issues**: https://github.com/LucasGeek/developer-evaluation/issues

---

<div align="center">

*Demonstrating Clean Architecture, CQRS, and DDD principles in .NET 8*

</div>
