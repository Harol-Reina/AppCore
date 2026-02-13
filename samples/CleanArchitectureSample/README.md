# Clean Architecture Sample

Reference implementation of a .NET 10 Web API built on **AppCore**, demonstrating Clean Architecture with CQRS, Dapper AOT, and NativeAOT compatibility.

## Architecture

```
ApiRest  (Presentation)     Minimal API endpoints, middleware, JSON context
    │
    ▼
Application  (Business)     CQRS commands/queries via MediatR, validation, DTOs
    │
    ▼
Infrastructure  (Data)      Dapper repositories, PostgreSQL, external HTTP services
```

Dependencies point inward — `Infrastructure` and `ApiRest` depend on `Application`, never the reverse.

## Project Structure

```
src/
├── ApiRest/
│   ├── Program.cs                  # App startup, DI, middleware pipeline
│   ├── SampleJsonContext.cs        # AOT-safe JSON serialization context
│   └── EndPoinds/
│       ├── EmployesEndpoint.cs     # CRUD endpoints for employees
│       └── PokemonsEndpoint.cs     # External API proxy endpoint
│
├── Application/
│   ├── DependencyInjection.cs      # MediatR + FluentValidation registration
│   ├── Domain/Entities/            # EmployeEntity, PokemonEntity
│   ├── DTOs/                       # Request/Response DTOs with validators
│   ├── Features/
│   │   ├── Employes/Command/       # Add, Edit, Delete commands + validators
│   │   ├── Employes/Query/         # GetAll (paginated), GetById queries
│   │   └── Pokemons/Query/         # GetAll (external API)
│   └── Interfaces/                 # Repository and service contracts
│
└── Infrastructure/
    ├── DependencyInjection.cs      # Repository + service bindings
    ├── Data/
    │   ├── DbInitializer.cs        # Auto-creates tables on startup
    │   └── NpgsqlConnectionFactory.cs
    ├── Repositories/               # Dapper-based data access
    └── Services/
        └── PokemonService.cs       # External PokeAPI integration
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (running locally or in Docker)

## Getting Started

1. **Start PostgreSQL** (e.g., via Docker):

   ```bash
   docker run -d --name postgres -p 5432:5432 \
     -e POSTGRES_USER=orion75 \
     -e POSTGRES_DB=appcore_sample \
     POSTGRES_HOST_AUTH_METHOD=trust \
     postgres:17
   ```

2. **Update the connection string** in `src/ApiRest/appsettings.json` if your setup differs:

   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=appcore_sample;Username=orion75"
   ```

3. **Run the application**:

   ```bash
   cd samples/CleanArchitectureSample
   dotnet run --project src/ApiRest
   ```

   The API starts on `http://localhost:5280`. Tables are created automatically by `DbInitializer`.

4. **Explore the API** via Swagger UI at `http://localhost:5280/swagger`.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/employes?page=1&pageSize=10&sort=Id&asc=true` | List employees (paginated) |
| `GET` | `/api/v1/employes/{id}` | Get employee by ID |
| `POST` | `/api/v1/employes` | Create employee |
| `PUT` | `/api/v1/employes` | Update employee |
| `DELETE` | `/api/v1/employes/{id}` | Delete employee |
| `GET` | `/api/v1/pokemons` | Fetch Pokemon list from PokeAPI |
| `GET` | `/health` | Health check |

## AppCore Features Demonstrated

| Feature | Usage in Sample |
|---------|----------------|
| `Response<T>` | Standardized API responses from all handlers |
| `PaginationResponse<T>` | Paginated employee listing with sort/direction |
| `ExceptionHandlingMiddleware` | Global exception-to-HTTP-response mapping |
| `NotFoundException` | Thrown when employee ID not found |
| `BaseEntity<T>` | Audit fields (CreatedAt/By, UpdatedAt/By) on entities |
| `HttpService` | Base class for `PokemonService` with trace ID + audit logging |
| `IDbConnectionFactory` | Abstracted database connection creation |
| `IMappingService<S,D>` | Entity-to-DAO bidirectional mapping |
| `IHttpRequestRepository` | Automatic HTTP request audit trail to PostgreSQL |
| `Configuration` | Centralized app settings access |
| `AppCoreJsonContext` | AOT-compatible JSON serialization |

## Database Schema

The application auto-creates two tables on startup:

**`Employes`** — domain data with audit trail

| Column | Type |
|--------|------|
| Id | SERIAL PRIMARY KEY |
| Name, Email, Phone | TEXT NOT NULL |
| CreatedAt | TIMESTAMP NOT NULL |
| CreatedBy, UpdatedAt, UpdatedBy | TEXT / TIMESTAMP |

**`HttpAudit`** — automatic HTTP request logging

| Column | Type |
|--------|------|
| Id | SERIAL PRIMARY KEY |
| TraceId | UUID NOT NULL |
| Endpoint, Method | TEXT NOT NULL |
| StatusCode | INTEGER |
| ElapsedMilliseconds | BIGINT NOT NULL |
| Headers, Body, Response | JSONB |
| InternalError | TEXT |
| CreatedAt | TIMESTAMP NOT NULL |
