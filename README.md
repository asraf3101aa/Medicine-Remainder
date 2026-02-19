# Medicine Reminder API

A production-grade, personal medicine reminder API built with **ASP.NET Core 10** following **Clean Architecture** principles and industry best practices.

## 🏗️ Architecture

The project follows the "Clean Architecture" (or Onion Architecture) pattern to ensure separation of concerns, testability, and independence from external frameworks:

- **Medicine.Domain**: Core entities, enums, and business logic (zero dependencies).
- **Medicine.Application**: Business logic, CQRS handlers (MediatR), DTOs, and interfaces.
- **Medicine.Infrastructure**: Data persistence (EF Core with SQLite) and external service implementations.
- **Medicine.Api**: Web API layer with Controllers, Middleware, and configuration.
- **Medicine.Application.UnitTests**: Automated tests for business logic.

## ✨ Features & Best Practices

- **.NET 10**: Leveraging the latest performance improvements and features.
- **CQRS Pattern**: Decoupled Reads and Writes using **MediatR**.
- **Validation**: Automatic request validation using **FluentValidation** integrated into the MediatR pipeline.
- **Global Error Handling**: Centralized middleware returning standard RFC 7807 `ProblemDetails`.
- **API Versioning**: Industry-standard versioning (e.g., `/api/v1/...`) via `Asp.Versioning`.
- **OpenAPI/Swagger**: Built-in support for API documentation and testing.
- **Logging**: Structured logging with **Serilog**.
- **Health Checks**: Built-in endpoints to monitor system and database health at `/health`.
- **Dependency Injection**: Clean registration of services in each layer.

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [dotnet-ef tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (for migrations)

### Setup

1. **Clone the repository** (if applicable).
2. **Build the solution**:
   ```bash
   dotnet build Medicine.slnx
   ```
3. **Run the API**:
   ```bash
   dotnet run --project Medicine.Api/Medicine.Api.csproj
   ```

### Database Migrations

The project uses SQLite for local development. To update the database:
```bash
dotnet ef database update --project Medicine.Infrastructure --startup-project Medicine.Api
```

## 🛠️ API Endpoints

### Medicines
- `POST /api/v1/Medicines`: Register a new medicine.
- `GET /api/v1/Medicines?userEmail={email}`: Retrieve all medicines for a specific user.

### Reminders
- `POST /api/v1/Reminders`: Schedule a new reminder for a medicine.

## 🧪 Testing

Run the unit tests suite:
```bash
dotnet test
```

## 📜 License

This project is licensed under the MIT License.
