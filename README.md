# MedicineReminder Reminder API

A production-grade, personal medicine reminder API built with **ASP.NET Core 10** following **Clean Architecture** principles and industry best practices.

## 🏗️ Architecture

The project follows the "Clean Architecture" (or Onion Architecture) pattern to ensure separation of concerns, testability, and independence from external frameworks:

- **MedicineReminder.Domain**: Core entities, enums, and business logic (zero dependencies).
- **MedicineReminder.Application**: Business logic, CQRS handlers (MediatR), DTOs, and interfaces.
- **MedicineReminder.Infrastructure**: Data persistence (EF Core with PostgreSQL) and external service implementations.
- **MedicineReminder.Api**: Web API layer with Controllers, Middleware, and configuration.
- **MedicineReminder.Application.UnitTests**: Automated tests for business logic.

## ✨ Features & Best Practices

- **.NET 10**: Leveraging the latest performance improvements and features.
- **PostgreSQL**: Industry-standard robust relational database.
- **JWT Authentication**: Secure API access with JSON Web Tokens and ASP.NET Core Identity.
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
   dotnet build MedicineReminder.slnx
   ```
3. **Run the API**:
   ```bash
   dotnet run --project MedicineReminder.Api/MedicineReminder.Api.csproj
   ```

### Database Migrations

The project uses SQLite for local development. To update the database:
```bash
dotnet ef database update --project MedicineReminder.Infrastructure --startup-project MedicineReminder.Api
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
