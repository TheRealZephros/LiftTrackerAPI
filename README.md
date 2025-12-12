# Lift Tracker API

A modular, extensible **.NET 8 Web API** for managing training programs, exercises, and logged workouts, including full auditing, authentication, and a structured repository architecture. This API supports creating structured programs, tracking progress over time, and capturing detailed exercise session data.

The system is designed for maintainability, observability, and clean separation of concerns, featuring:

- Entity Framework Core (SQL Server + InMemory for tests)
- JWT-based authentication
- ASP.NET Identity for user management
- Repository pattern for data access
- Serilog logging (Console, File, Seq)
- Auditing interceptor with a dedicated `AuditDbContext`
- Seed-data bootstrapping
- Comprehensive automated tests

---

## Table of Contents

1. [Overview](#1-overview)  
2. [Key Features](#2-key-features)  
3. [Architecture](#3-architecture)  
4. [Project Structure](#4-project-structure)  
5. [Technology Stack](#5-technology-stack)  
6. [Installation and Setup](#6-installation-and-setup)  
7. [Configuration](#7-configuration)  
8. [Database and Migrations](#8-database-and-migrations)  
9. [Auditing System](#9-auditing-system)  
10. [Logging](#10-logging)  
11. [Running the Application](#11-running-the-application)  
12. [Testing](#12-testing)  
13. [API Overview](#13-api-overview)  
14. [Docker Support](#14-docker-support)

---

## 1. Overview

The Lift Tracker API enables users to:

- Define **Exercises** (e.g., Bench Press, Deadlift)
- Create multi-day **Training Programs** with programmed exercises
- Log **Exercise Sessions** and **Exercise Sets**
- Track historical performance and progress
- Authenticate securely using JWT tokens
- Maintain rich audit trails of all data-changing operations

The system is suited for fitness applications, structured workout tracking tools, and platforms requiring historical progress visibility.

---

## 2. Key Features

### Training Features
- CRUD operations for Exercises, Sessions, and Training Programs  
- Support for Program Days and Programmed Exercises  
- Logging of exercise sets with reps, weight, RPE, etc.

### Authentication
- ASP.NET Identity for user and role management  
- JWT-based login, refresh tokens, and session protections

### Observability
- Serilog sinks: Console, File rotation, Seq integration  
- Correlation ID propagation

### Auditing
- Custom EF Core `SaveChangesInterceptor`  
- `AuditDbContext` stores inserts, updates, deletes  
- Per-property old/new value tracking  
- Multi-entity transaction auditing

### Testing
- xUnit-based test suite  
- Moq for isolation  
- EF Core InMemory provider for repository tests  
- Auditing interceptor tests, controller tests, repository tests, smoke tests

---

## 3. Architecture

### Layered Architecture
- API Layer (Controllers)
- DTO Layer
- Mapper Layer
- Repository Layer
- Infrastructure (Auditing, Http, Security, Correlation)
- EF Core DbContexts
- Domain Models


### DbContexts

#### ApplicationDbContext  
Primary runtime DB for all application entities.

#### AuditDbContext  
Separate schema (`audit`) storing audit logs with isolated migration history.

### Cross-Cutting Concerns
- Correlation IDs generated per request  
- UserContext injected into services and interceptors  
- HttpContextInfo captures environmental metadata  

### Middleware Pipeline (Abbreviated)
- Serilog request logging  
- Correlation ID middleware  
- Authentication and Authorization  
- MVC routing  

---

## 4. Project Structure

### `/Api` — Primary API Application

#### Controllers
Handles HTTP operations:
- ExerciseController  
- ExerciseSessionController  
- TrainingProgramController  
- UserController  

#### Data
- `ApplicationDbContext`  
- `AuditDbContext`  
- `DbInitializer` (roles, admin, seed exercises, seed sessions)

#### Infrastructure
- **Auditing:** `AuditSaveChangesInterceptor`  
- **Security:** `UserContext`  
- **Http:** `HttpContextInfo`  
- **Correlation:** `CorrelationIdAccessor`  

#### DTOs
Segmented by domain (Exercise, ExerciseSession, TrainingProgram, User)

#### Repositories
Implements repository pattern:
- ExerciseRepository  
- TrainingProgramRepository  
- ExerciseSessionRepository  
- UserRepository  

#### Mappers
Entity ↔ DTO mapping

#### Models
Domain entities:
- Exercise  
- ExerciseSession  
- ExerciseSet  
- TrainingProgram  
- ProgramDay  
- ProgrammedExercise  
- User  
- RefreshToken  
- AuditLog  

---

## 5. Technology Stack

| Component | Technology |
|----------|------------|
| Framework | .NET 8 |
| Language | C# |
| API Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Authentication | JWT (HMAC-SHA512) + ASP.NET Identity |
| Logging | Serilog + Seq |
| Testing Frameworks | xUnit, Moq |
| Serialization | Newtonsoft.Json |

---

## 6. Installation and Setup

### Prerequisites
- .NET 8 SDK  
- SQL Server (LocalDB, Express, or remote)  
- Optional: Docker + Docker Compose  
- Optional: Seq for log ingestion  

### Clone the Repository

```sh
git clone <repo-url>
cd lift_tracker_api
```

### Restore Dependencies

```sh
dotnet restore
```

## Configuration

### appsettings.json

important fields:

```json
"ConnectionStrings": {
  "DefaultConnection": "<SQL connection string>"
},
"JWT": {
  "Issuer": "...",
  "Audience": "...",
  "SigningKey": "...",
  "ExpiryDays": 7
}
```

### Environment Variable Overrides

Example .env (TBD):
```sh
ConnectionStrings__DefaultConnection=
JWT__Issuer=
JWT__Audience=
JWT__SigningKey=
JWT__ExpiryDays=
```

## 8. Database and Migrations

### Apply Migrations

```sh
dotnet ef database update --project Api
```

### Add A New Migration

```sh
dotnet ef migrations add <Name> --project Api
```

### Seperate Audit Migrations

The `AuditDbContext` uses a dedicated schema and its own migrations history table:
```sh
sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "audit");
```

## 9. Auditing System

### Overview

Auditing is implemented with a custom `AuditSaveChangesInterceptor`. It triggers whenever EF Core performs:
-Insert
-Update
-Delete

Each change is captured and written into `AuditDbContext`.

### AuditLog Record Includes
Each audit entry contains:
- Entity/table name
- Entity primary key
- Action type (`ADDED`, `MODIFIED`, `DELETED`)
- Old values (JSON)
- New values (JSON)
- List of changed properties
- Timestamp
- Correlation ID
- User ID
- HTTP metadata (IP, user agent, etc.)

### Multi-Entity Audits

If a single `SaveChanges()` call modifies several entities, multiple audit rows are created within the same transaction.

### Negative Audit Behavior

When `SaveChanges()` is called without any tracked modifications, no audit logs are written.

## 10. Logging
The application uses Serilog with configuration driven by `appsettings.json` and `Program.cs`.

### Logging Sinks

- Console
- Rolling file logs (14-day retention)
- Seq (`http://localhost:5341`)

### Correlation Enrichment
Each incoming request is assigned or forwarded a `X-Correlation-ID`, which is included in:

- Log entries
- Audit logs
- Diagnostic context enrichment

Middleware ensures correlation IDs propagate consistently.

## 11. Running the Application

### Development
```sh
cd Api
dotnet run
```

### Swagger UI
When running in Development mode:
```sh
http://localhost:<port>/swagger
```

## 12. Testing
The solution includes a comprehensive test suite using:

- xUnit
- Moq
- EF Core InMemory provider
- Microsoft.NET.Test.Sdk

### Run All Tests
To run all tests from the project root:
```sh
dotnet test .\Tests\tests.csproj
```

### Test Coverage Includes

- Repository tests
- Controller tests
- Auditing interceptor tests
- Negative auditing tests
- Multi-entity auditing tests
- Smoke tests validating end-to-end behavior

# 13. API Overview

This section provides a structured overview of all API capabilities grouped by controller.  
It describes authentication requirements, ownership rules, and every available endpoint in a concise, readable format.

For complete DTO definitions and live examples, refer to Swagger UI (`/swagger`) when running the API in development mode.

---

# 13.1 Authentication & User Management (`UserController`)

Handles login, registration, token refresh, and administrative deletion of users.

### Authentication Requirements
- Login, register, and refresh are anonymous.
- Delete currently has no authentication guard (this can be adjusted depending on system requirements).

---

## Endpoints

### User Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/user/login` | Validate credentials, issue access + refresh tokens | No |
| POST | `/api/user/refresh` | Exchange a valid refresh token for a new access token | No |

### User Registration
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/user/register` | Register a new user, assign "User" role, return tokens | No |

### User Deletion
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| DELETE | `/api/user/delete?id={userId}` | Permanently delete a user | No (but typically restricted in production) |

---

## Behavior Notes
- Refresh tokens are stored in the database with expiration and revocation logic.
- Credentials are validated using ASP.NET Identity.
- Login failure does not reveal whether the email or password was incorrect.
- Registration auto-assigns the `User` role.
- Login and registration return both access and refresh tokens.

---

# 13.2 Exercises (`ExerciseController`)

Manages user-created exercises and prevents deletion if an exercise is referenced by sessions or training programs.

### Authentication Requirements
All endpoints require a valid JWT access token.

### Ownership Rules
- Users can only access their own exercises.
- Unauthorized access returns `404` (intentional security behavior).
- Exercises associated with sessions or programmed exercises cannot be deleted.

---

## Endpoints

### Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/exercises` | Get all exercises for the authenticated user |
| GET | `/api/exercises/{id}` | Get one exercise by ID |

### Creation
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/exercises/create` | Create a new exercise |

### Update
| Method | Endpoint | Description |
|--------|----------|-------------|
| PUT | `/api/exercises/update/{id}` | Update an exercise |

### Delete
| Method | Endpoint | Description |
|--------|----------|-------------|
| DELETE | `/api/exercises/delete/{id}` | Delete an exercise (only if unused) |

---

## Behavior Notes
- Unauthorized access yields `404 NotFound`.
- Validation failures return `400 BadRequest`.
- DTO mapping is handled with mapper extensions.

---

# 13.3 Exercise Sessions & Exercise Sets (`ExerciseSessionController`)

Allows users to track workout history: sessions and sets (reps, weight, RPE, etc.).

### Authentication Requirements
All endpoints require a valid JWT.

### Ownership Rules
- Users may only access their own sessions and sets.
- Access to another user’s data returns `404`.

---

## Endpoints

### Session Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sessions` | Retrieve all sessions for the user |
| GET | `/api/sessions/{sessionId}` | Retrieve a session by ID |
| GET | `/api/sessions/exercise/{exerciseId}` | Retrieve all sessions associated with a specific exercise |

### Set Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sessions/{sessionId}/sets` | Get all sets belonging to a session |
| GET | `/api/sessions/sets/{setId}` | Get a specific set |

### Creating
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/create` | Create a new exercise session |
| POST | `/api/sessions/sets/create` | Create a new set inside a session |

### Updating
| Method | Endpoint | Description |
|--------|----------|-------------|
| PUT | `/api/sessions/update/{sessionId}` | Update a session |
| PUT | `/api/sessions/sets/update/{setId}` | Update a set |

### Deleting
| Method | Endpoint | Description |
|--------|----------|-------------|
| DELETE | `/api/sessions/delete/{sessionId}` | Delete a session |
| DELETE | `/api/sessions/sets/delete/{setId}` | Delete a set |

---

## Behavior Notes
- Creating a session validates that the exercise belongs to the calling user.
- Creating a set validates that the session belongs to the user.
- All mapping is handled through DTO mappers.
- Unauthorized access returns `404 NotFound`.

---

# 13.4 Training Programs (`TrainingProgramController`)

Allows users to create structured, multi-day training programs with ordered programmed exercises.

A Training Program consists of:

- **TrainingProgram** (root program)
- **ProgramDay** (child days)
- **ProgrammedExercise** (exercise assigned to a day with a position/order)

### Authentication Requirements
All endpoints require a valid JWT.

### Ownership Rules
- Users can only access programs they own.
- Days and programmed exercises validate correct owner via repository-level checks.

---

## Endpoints

### Program Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/programs` | Get all programs for the user |
| GET | `/api/programs/{programId}` | Get one program (including days) |

### Day Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/programs/program/{programId}/days` | Get all days of a program |
| GET | `/api/programs/days/{dayId}` | Get a specific day |
| GET | `/api/programs/days/{dayId}/exercises` | Get all programmed exercises on a day |

### Programmed Exercise Retrieval
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/programs/exercises/{id}` | Get a specific programmed exercise |

---

### Creation Operations
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/programs/create` | Create a new training program |
| POST | `/api/programs/days/create` | Create a new program day |
| POST | `/api/programs/exercises/create` | Create a new programmed exercise in a day |

Validation includes:
- Program existence and ownership  
- Day existence and ownership  
- Exercise existence and ownership  
- Preventing duplicate positions inside a Program Day  

---

### Update Operations
| Method | Endpoint | Description |
|--------|----------|-------------|
| PUT | `/api/programs/update/{programId}` | Update a training program |
| PUT | `/api/programs/update/days/{dayId}` | Update a program day |
| PUT | `/api/programs/update/exercises/{id}` | Update a programmed exercise |

---

### Delete Operations
| Method | Endpoint | Description |
|--------|----------|-------------|
| DELETE | `/api/programs/delete/{programId}` | Delete a complete training program (including all days) |
| DELETE | `/api/programs/delete/days/{dayId}` | Delete a program day |
| DELETE | `/api/programs/delete/exercises/{id}` | Delete a programmed exercise |

---

## Behavior Notes
- Unauthorized access always returns `404` rather than `403`.
- Cascade delete (program → days → exercises) is performed manually in the controller via repository calls.
- DTOs are projected using mapper extensions.
- All updates return `204 NoContent` unless they return DTOs.

---

# 13.5 Common API Behaviors

### Success Responses
- `200 OK` — Standard retrieval responses  
- `201 Created` — Resource creation  
- `204 NoContent` — Successful deletes or updates without payload  

### Error Responses
- `400 BadRequest` — Invalid input or domain validation failure  
- `401 Unauthorized` — Missing/invalid JWT  
- `404 NotFound` — Resource missing or unauthorized access  
- `500 InternalServerError` — Unexpected server errors  

### Authentication Header

```
Authorization: Bearer <JWT>
```


## 14. Docker Support

The repository includes a `docker-compose.yml` enabling:

- API container
- SQL Server database container
- Optional Seq container for log ingestion

### Start The Stack
```sh
docker compose up --build
```
