# Developer Evaluation - Sales API 🍺

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)](https://www.postgresql.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-8.0-green)](https://www.mongodb.com/)
[![Redis](https://img.shields.io/badge/Redis-7.4-red)](https://redis.io/)

A comprehensive **Sales Management API** built with **.NET 8** implementing **Clean Architecture**, **CQRS pattern**, and **Domain-Driven Design (DDD)** principles. This project features a complete sales system with automatic discount calculations, inventory management, and real-time data synchronization.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Seed Data](#seed-data)
- [API Documentation](#api-documentation)
- [Business Rules](#business-rules)
- [Testing](#testing)
- [Performance](#performance)
- [Security](#security)
- [Contributing](#contributing)

## 🎯 Overview

This project serves as a **senior developer evaluation**, demonstrating proficiency in:

- **Clean Architecture** with proper layer separation
- **CQRS** (Command Query Responsibility Segregation) pattern
- **Event-driven architecture** with domain events
- **RESTful API** design with comprehensive CRUD operations
- **Multi-database** architecture (PostgreSQL + MongoDB + Redis)
- **Automated testing** with comprehensive coverage
- **Security** best practices and authentication
- **Performance optimization** and caching strategies

### Core Business Domain

The API manages a **Sales System** for a product catalog, handling:

- **Sales Management**: Complete CRUD operations for sales records
- **Product Catalog**: Beverage and snack inventory
- **Shopping Carts**: Customer cart management with real-time updates
- **User Management**: Role-based access control (Admin, Manager, Customer)
- **Discount Engine**: Automatic discount calculations based on quantity tiers
- **Inventory Control**: Stock validation and business rule enforcement

## ✨ Features

### 🛒 Sales Management
- Complete **CRUD operations** for sales records
- **Real-time discount calculations** based on quantity rules
- **Inventory validation** with business rule enforcement
- **Sales cancellation** with proper status tracking
- **Multi-item sales** with individual item management

### 📦 Product Catalog
- **Comprehensive product management** with categories
- **Real-time inventory tracking**
- **Product search and filtering** by category, price, rating
- **Pagination and sorting** capabilities
- **Rating system** with aggregated statistics

### 🛍️ Shopping Cart System
- **Persistent cart management** across sessions
- **Real-time price calculations** with discounts
- **Cart sharing** and collaborative shopping
- **Auto-save functionality** for user convenience

### 👥 User Management
- **Role-based access control** (Admin, Manager, Customer)
- **JWT authentication** with secure token handling
- **User profile management** with status tracking
- **Password encryption** using industry-standard hashing

### 🔄 CQRS & Event Sourcing
- **Command-Query separation** for optimal performance
- **Event-driven synchronization** between write and read models
- **Real-time data consistency** across multiple databases
- **Audit trail** with complete operation history

### 🚀 Performance & Scalability
- **Redis caching** for frequently accessed data
- **Database optimization** with proper indexing
- **Async operations** throughout the application
- **Horizontal scaling** ready architecture

## 🛠️ Tech Stack

### Backend
- **[.NET 8.0](https://dotnet.microsoft.com/)** - Latest LTS version with performance improvements
- **[C# 12](https://docs.microsoft.com/en-us/dotnet/csharp/)** - Modern language features and syntax
- **[ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)** - High-performance web framework

### Architecture & Patterns
- **[MediatR](https://github.com/jbogard/MediatR)** - CQRS and mediator pattern implementation
- **[AutoMapper](https://github.com/AutoMapper/AutoMapper)** - Object-to-object mapping
- **[FluentValidation](https://fluentvalidation.net/)** - Input validation with fluent syntax

### Databases
- **[PostgreSQL 15](https://www.postgresql.org/)** - Primary OLTP database for write operations
- **[MongoDB 8.0](https://www.mongodb.com/)** - Document store for read models and reporting
- **[Redis 7.4](https://redis.io/)** - In-memory cache for performance optimization

### Testing
- **[xUnit](https://xunit.net/)** - Unit testing framework
- **[NSubstitute](https://nsubstitute.github.io/)** - Mocking framework for test isolation
- **[Bogus](https://github.com/bchavez/Bogus)** - Test data generation

### DevOps & Tools
- **[Docker](https://www.docker.com/)** - Containerization for consistent environments
- **[Serilog](https://serilog.net/)** - Structured logging for observability
- **[Swagger/OpenAPI](https://swagger.io/)** - API documentation and testing

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation Layer                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   WebApi        │  │   Controllers   │  │  Middleware  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                       Application Layer                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Commands      │  │    Queries      │  │   Handlers   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                         Domain Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Entities      │  │  Business Rules │  │    Events    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  Repositories   │  │   Data Access   │  │   External   │ │
│  │                 │  │   (EF Core)     │  │   Services   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Flow

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Command   │───▶│  PostgreSQL │───▶│   Events    │
│   (Write)   │    │ (Write DB)  │    │  (Domain)   │
└─────────────┘    └─────────────┘    └─────────────┘
                                              │
                                              ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│    Query    │◀───│   MongoDB   │◀───│   Handlers  │
│   (Read)    │    │  (Read DB)  │    │ (Sync Data) │
└─────────────┘    └─────────────┘    └─────────────┘
```

## 🚀 Getting Started

### Prerequisites

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or later
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** for database containers
- **[Git](https://git-scm.com/)** for version control
- **IDE**: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/developer-evaluation.git
   cd developer-evaluation/template/backend
   ```

2. **Start the database containers**
   ```bash
   docker-compose up -d
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
   ```

5. **Start the application**
   ```bash
   dotnet run --project src/DeveloperEvaluation.WebApi
   ```

6. **Access the API**
   - **Swagger UI**: http://localhost:5001/swagger
   - **API Base**: http://localhost:5001/api
   - **Health Check**: http://localhost:5001/health

## 🗄️ Database Setup

### Docker Compose Services

The application uses Docker Compose to orchestrate multiple databases:

```yaml
services:
  # PostgreSQL - Primary database for write operations
  postgres:
    image: postgres:15
    ports: ["5432:5432"]
    environment:
      POSTGRES_DB: developer_evaluation
      POSTGRES_USER: developer
      POSTGRES_PASSWORD: ev@luAt10n

  # MongoDB - Read models and reporting
  mongodb:
    image: mongo:8.0
    ports: ["27017:27017"]

  # Redis - Caching and session storage
  redis:
    image: redis:7.4.1-alpine
    ports: ["6379:6379"]
    command: redis-server --requirepass ev@luAt10n
```

### Database Commands

```bash
# Start all services
docker-compose up -d

# View service status
docker-compose ps

# Stop all services
docker-compose down

# Reset databases (WARNING: destroys data)
docker-compose down -v && docker-compose up -d
```

### Entity Framework Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi

# Apply migrations
dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi

# Reset database
dotnet ef database drop -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
```

## 🌱 Seed Data

The application includes a comprehensive **seed data system** that populates the database with realistic test data. **Seeding is now executed manually** as a separate command to keep the application startup clean and fast.

### 🎯 What Gets Seeded

#### 👥 **Users (5 total)**
- **1 Admin**: `admin@developereval.com` / `Admin123!`
- **1 Manager**: `manager@developereval.com` / `Manager123!`
- **3 Customers**: `customer1@developereval.com`, `customer2@developereval.com`, `customer3@developereval.com` / `Customer123!`

#### 🍺 **Products (17 total)**
Authentic **product catalog** including:

- **Beverages**: Budweiser, Stella Artois, Corona, Brahma, Antarctica Pilsen
- **Premium Beverages**: Beck's, Leffe, Hoegaarden
- **Snacks**: Doritos, Lay's, Pringles, Cheetos
- **Soft Drinks**: Guaraná Antarctica, Pepsi, H2OH
- **Energy Drinks**: Red Bull, Monster Energy

Each product includes:
- Realistic pricing ($2.99 - $15.99)
- Product descriptions and categories
- Random ratings (3.5-5.0 stars)
- Review counts (100-2500 reviews)

#### 🛒 **Shopping Carts (3 total)**
- Random carts for customer users
- 2-5 products per cart with varied quantities
- Realistic shopping scenarios

#### 💰 **Sales (15 total)**
- Sales across 5 different branch locations in Brazil
- 1-4 products per sale with business logic applied
- Automatic discount calculations
- 15% of sales randomly cancelled for testing
- Varied quantities to test all discount rules

### 🔧 Running Seed Data

**Execute seeding manually** using the dedicated command:

```bash
# Run seeding command (executes once and exits)
dotnet run --project src/DeveloperEvaluation.WebApi --seed

# OR from the WebApi directory
cd src/DeveloperEvaluation.WebApi
dotnet run --seed
```

**Note**: The `--seed` command will populate the database and then exit. This keeps the regular `dotnet run` command clean and fast.

#### Manual Seed Operations

```bash
# 1. Apply database migrations first
dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi

# 2. Run seeding
dotnet run --project src/DeveloperEvaluation.WebApi --seed

# 3. Start the application normally
dotnet run --project src/DeveloperEvaluation.WebApi

# Force re-seed (drops and recreates data)
dotnet ef database drop -f -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
dotnet ef database update -p src/DeveloperEvaluation.ORM -s src/DeveloperEvaluation.WebApi
dotnet run --project src/DeveloperEvaluation.WebApi --seed
```

#### Seed Data Features

- **Manual Execution**: Run only when needed, keeping application startup fast
- **Idempotent**: Won't create duplicates if data already exists
- **Realistic**: Uses realistic product names and data
- **Business Logic**: Applies all discount rules and validation
- **Comprehensive Logging**: Detailed logs for troubleshooting
- **Error Handling**: Graceful failure without breaking the application

### 🎨 Using Seed Data

The seed data provides **immediate functionality** for testing:

```bash
# Test the API immediately after startup
curl http://localhost:5001/api/products

# Login with seeded admin user
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@developereval.com","password":"Admin123!"}'

# View seeded sales
curl http://localhost:5001/api/sales \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📚 API Documentation

### 🌐 Swagger UI

The API includes **comprehensive Swagger documentation** with interactive testing capabilities:

- **URL**: http://localhost:5001/swagger
- **Features**:
  - Interactive API testing
  - Request/response examples
  - Schema documentation
  - Authentication testing
  - Error response examples

### 🔑 Authentication

All endpoints (except health checks) require **JWT authentication**:

```bash
# 1. Login to get token
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@developereval.com","password":"Admin123!"}'

# 2. Use token in subsequent requests
curl http://localhost:5001/api/products \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 📋 Core Endpoints

#### 🔐 **Authentication**
```
POST   /api/auth/login          # User authentication
```

#### 👥 **Users**
```
GET    /api/users               # List users (Admin/Manager only)
POST   /api/users               # Create user (Admin only)
GET    /api/users/{id}          # Get user details
PUT    /api/users/{id}          # Update user
DELETE /api/users/{id}          # Delete user (Admin only)
```

#### 📦 **Products**
```
GET    /api/products            # List products with filters
POST   /api/products            # Create product (Manager/Admin)
GET    /api/products/{id}       # Get product details
PUT    /api/products/{id}       # Update product (Manager/Admin)
DELETE /api/products/{id}       # Delete product (Manager/Admin)
GET    /api/products/categories # List categories
GET    /api/products/category/{category} # Products by category
```

#### 🛒 **Shopping Carts**
```
GET    /api/carts               # List carts (Manager/Admin)
POST   /api/carts               # Create cart
GET    /api/carts/{id}          # Get cart details
PUT    /api/carts/{id}          # Update cart
DELETE /api/carts/{id}          # Delete cart
```

#### 💰 **Sales**
```
GET    /api/sales               # List sales
POST   /api/sales               # Create sale
GET    /api/sales/{id}          # Get sale details
PUT    /api/sales/{id}          # Update sale
DELETE /api/sales/{id}          # Cancel sale
```

### 📊 Query Parameters

All list endpoints support:

- **Pagination**: `?page=1&size=20`
- **Sorting**: `?sort=price desc,title asc`
- **Filtering**: `?category=beverages&minPrice=5&maxPrice=20`

### 📝 Request/Response Examples

#### Create Sale
```json
POST /api/sales
{
  "saleNumber": "SALE-2024-001",
  "customerId": "guid",
  "customerDescription": "John Doe",
  "branchId": "guid",
  "branchDescription": "São Paulo - Centro",
  "items": [
    {
      "productId": "guid",
      "productDescription": "Budweiser Lager Beer",
      "quantity": 12,
      "unitPrice": 8.99
    }
  ]
}
```

#### Response with Discounts Applied
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "saleNumber": "SALE-2024-001",
    "date": "2024-01-15T10:30:00Z",
    "customerId": "guid",
    "customerDescription": "John Doe",
    "branchId": "guid",
    "branchDescription": "São Paulo - Centro",
    "totalAmount": 86.31,
    "cancelled": false,
    "items": [
      {
        "productId": "guid",
        "productDescription": "Budweiser Lager Beer",
        "quantity": 12,
        "unitPrice": 8.99,
        "discount": 21.58,
        "total": 86.31
      }
    ]
  }
}
```

## 📋 Business Rules

### 💸 Discount Calculation

The system implements **automatic discount calculation** based on quantity tiers:

| Quantity Range | Discount | Example |
|---------------|----------|---------|
| 1-3 items | **0%** | 3 × $10.00 = **$30.00** |
| 4-9 items | **10%** | 6 × $10.00 = **$54.00** (was $60.00) |
| 10-20 items | **20%** | 15 × $10.00 = **$120.00** (was $150.00) |
| 21+ items | **❌ Not Allowed** | Maximum 20 items per product |

### ⚠️ Validation Rules

#### Sales Validation
- **Maximum 20 items** per product per sale
- **No discounts** for quantities below 4 items
- **Automatic discount application** for eligible quantities
- **Stock validation** before sale completion

#### User Validation
- **Strong password policy**: 8+ chars, uppercase, lowercase, number, special char
- **Unique email addresses** across all users
- **Valid phone number format**: Brazilian format
- **Role-based access control** enforcement

#### Product Validation
- **Positive pricing** validation
- **Required fields**: title, description, category
- **Price range**: $0.01 - $999.99
- **Category validation** against predefined list

### 🎯 Business Logic Examples

```csharp
// Automatic discount calculation
var saleItem = new SaleItem(saleId, productId, "Budweiser", 12, 8.99m);
saleItem.ApplyDiscount(); // Automatically applies 20% discount
// Result: Total = $86.31 (was $107.88)

// Quantity validation
var sale = new Sale();
var item1 = new SaleItem(saleId, productId, "Corona", 15, 7.99m);
var item2 = new SaleItem(saleId, productId, "Corona", 10, 7.99m); // Would exceed 20 total
sale.AddItem(item1); // ✅ Success
sale.AddItem(item2); // ❌ Throws InvalidOperationException
```

## 🧪 Testing

### Test Structure

```
tests/
├── DeveloperEvaluation.Unit/        # Unit tests
├── DeveloperEvaluation.Integration/ # Integration tests
└── DeveloperEvaluation.Functional/  # API tests
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/DeveloperEvaluation.Unit/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with detailed output
dotnet test --verbosity normal
```

### Test Coverage

The project maintains **high test coverage** across:

- **Domain Logic**: 95%+ coverage of business rules
- **Application Services**: 90%+ coverage of CQRS handlers
- **API Controllers**: 85%+ coverage of endpoints
- **Integration Tests**: Complete database and API workflows

### Test Categories

#### Unit Tests (153 tests)
- **Domain Entity Validation**: Business rules and invariants
- **Discount Calculation**: All quantity-based scenarios
- **CQRS Handlers**: Command and query processing
- **Validation Logic**: Input validation and constraints

#### Integration Tests
- **Database Operations**: Entity Framework and MongoDB
- **CQRS Flow**: Command → Event → Read Model synchronization
- **Authentication**: JWT token generation and validation
- **Caching**: Redis integration and cache invalidation

#### Functional Tests
- **End-to-End API**: Complete request/response cycles
- **Authentication Flow**: Login and protected endpoints
- **Business Scenarios**: Real-world usage patterns
- **Error Handling**: Comprehensive error response testing

## ⚡ Performance

### Optimization Strategies

#### Database Performance
- **Indexed queries** on frequently searched fields
- **Efficient joins** with proper foreign key relationships
- **Connection pooling** for optimal resource usage
- **Query optimization** with Entity Framework best practices

#### Caching Strategy
- **Redis caching** for frequently accessed products
- **In-memory caching** for configuration and lookup data
- **Cache invalidation** on data updates
- **Cache-aside pattern** implementation

#### Async Operations
- **Async/await** throughout the application stack
- **Non-blocking I/O** for database and external service calls
- **Parallel processing** where applicable
- **Efficient resource utilization**

### Performance Metrics

```bash
# Typical response times (localhost)
GET  /api/products          # ~50ms  (with Redis cache)
POST /api/sales             # ~150ms (includes validation + CQRS)
GET  /api/sales/{id}        # ~30ms  (read from MongoDB)
POST /api/auth/login        # ~200ms (includes password hashing)
```

### Monitoring

- **Structured logging** with Serilog
- **Health checks** for all dependencies
- **Request/response logging** for audit trails
- **Performance counters** for key operations

## 🔒 Security

### Authentication & Authorization

- **JWT Bearer tokens** with configurable expiration
- **Role-based access control** (Admin, Manager, Customer)
- **Password hashing** using secure algorithms
- **Token validation** on all protected endpoints

### Security Headers

```csharp
// Applied security headers
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(/* Security headers middleware */);
```

### Input Validation

- **FluentValidation** for comprehensive input validation
- **SQL injection protection** via Entity Framework parameterization
- **XSS protection** through input sanitization
- **Request size limits** to prevent DoS attacks

### Environment Configuration

```json
// Secure configuration management
{
  "ConnectionStrings": {
    "DefaultConnection": "ENVIRONMENT_VARIABLE_REQUIRED"
  },
  "Jwt": {
    "SecretKey": "REPLACE_WITH_SECURE_KEY_FROM_ENVIRONMENT_VARIABLES"
  },
  "AllowedHosts": "localhost;*.localhost"
}
```

## 🤝 Contributing

### Development Workflow

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Make your changes** following the coding standards
4. **Write/update tests** for new functionality
5. **Ensure all tests pass**: `dotnet test`
6. **Commit your changes**: `git commit -m 'feat: add amazing feature'`
7. **Push to the branch**: `git push origin feature/amazing-feature`
8. **Open a Pull Request**

### Coding Standards

- **Clean Code** principles
- **SOLID** design principles
- **Domain-Driven Design** patterns
- **Comprehensive unit testing**
- **XML documentation** for public APIs
- **Consistent naming conventions**

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Code Review Checklist

- [ ] **Business logic** is properly tested
- [ ] **Security** considerations addressed
- [ ] **Performance** impact evaluated
- [ ] **Documentation** updated
- [ ] **SOLID principles** followed
- [ ] **Error handling** implemented
- [ ] **Logging** added where appropriate

---

## 📞 Support

For questions, issues, or contributions:

- **GitHub Issues**: [Create an issue](https://github.com/your-username/developer-evaluation/issues)
- **Documentation**: Check the `/docs` folder for detailed guides
- **Code Review**: All contributions welcome via Pull Requests

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Built with ❤️ for Developer Evaluation**

*Demonstrating Clean Architecture, CQRS, and DDD principles in .NET 8*

</div>