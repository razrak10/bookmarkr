# Bookmarkr CLI Application

Bookmarkr is a command-line interface (CLI) application for managing bookmarks. This document outlines the technologies, architectural patterns, and key features used in its development, serving as a technical reference.

## Core Technologies

- **Framework:** .NET 9.0
- **CLI Framework:** `System.CommandLine` for parsing arguments and building a rich command structure.
- **Database:** SQLite with Entity Framework Core for object-relational mapping (ORM).
- **Logging:** `Serilog` for structured and configurable logging.
- **Dependency Injection:** `Microsoft.Extensions.DependencyInjection` for managing object lifecycles and dependencies.
- **Hosting:** `Microsoft.Extensions.Hosting` for a generic host setup that bootstraps the application.
- **HTTP:** `IHttpClientFactory` for resilient and manageable HTTP requests to external services.

## Architectural Patterns & Concepts

The application is structured using several well-established software design patterns to ensure it is maintainable, testable, and scalable.

### Dependency Injection (DI)

DI is used extensively to decouple components. In [`Program.cs`](src/bookmarkr/Program.cs), services, repositories, and command handlers are registered with the service container. This allows dependencies to be "injected" via constructors, making it easy to swap implementations or mock them for testing.

```csharp
// filepath: src/bookmarkr/Program.cs
// ...existing code...
IHost host = Host.CreateDefaultBuilder(args)
     .ConfigureServices((hostcontext, services) =>
     {
         // Add Entity Framework
         services.AddDbContext<BookmarkrDbContext>(options => options.UseSqlite("Data Source=bookmarks.db"));

         // Register custom services and repositories
         services.AddScoped<IBookmarkRepository, BookmarkRepository>();
         services.AddScoped<IBookmarkService, BookmarkService>();

         // Register command handlers
         services.AddTransient<LinkAddCommandHandler>();
         // ... more handlers
     })
     .Build();
// ...existing code...
```

### Repository Pattern

Data access logic is abstracted using the Repository pattern. The [`IBookmarkRepository`](src/bookmarkr/Persistence/BookmarkRepository.cs) interface defines the data operations, and [`BookmarkRepository`](src/bookmarkr/Persistence/BookmarkRepository.cs) provides the EF Core implementation. This separates the business logic from the data access technology.

### Service Layer

A service layer, represented by [`IBookmarkService`](src/bookmarkr/Service/BookmarkService.cs), encapsulates the core business logic. It orchestrates operations by coordinating between the command handlers and the repository, ensuring a clean separation of concerns.

### Command Pattern (CLI)

The application's CLI is built using `System.CommandLine`. Each command (e.g., `link`, `import`) is mapped to a handler class (e.g., [`ImportCommandHandler`](src/bookmarkr/Commands/Import/ImportCommandHandler.cs)). This pattern is configured in the `ConfigureCommands` method in [`Program.cs`](src/bookmarkr/Program.cs), where commands, subcommands, and their corresponding handlers are defined and wired up.

### Service Agent Pattern

Communication with external services is encapsulated in Service Agents. For example, [`IBookmarkrSyncrServiceAgent`](src/bookmarkr/ServiceAgent/BookmarkrSyncrServiceAgent.cs) defines a contract for interacting with a remote sync API. This isolates external dependencies and makes the core application independent of specific external service implementations.

## Key Features & Implementation

- **Database Migrations:** EF Core migrations are used to manage the database schema. The `EnsureDatabaseAsync` method in [`Program.cs`](src/bookmarkr/Program.cs) automatically applies any pending migrations on startup.

- **Structured Logging:** `Serilog` is configured via `appsettings.json` to provide structured logging to files. A simple [`LogManager`](src/bookmarkr/Logger/LogManager.cs) class provides a static wrapper for easy access to the logger.

- **Graceful Shutdown:** The application handles process exit events (`ProcessExit`, `CancelKeyPress`) to ensure tasks like flushing logs are completed before termination, as seen in the `FreeSerilogLoggerOnShutdown` method in [`Program.cs`](src/bookmarkr/Program.cs).

- **Asynchronous Operations:** The entire application stack, from command handlers to the database, is built with `async`/`await` to ensure the CLI remains responsive during I/O-bound operations like database queries and HTTP requests.

- **HTTP Client Management:** `IHttpClientFactory` is used to manage `HttpClient` instances. A named client, `bookmarkrSyncr`, is configured with a base address and default headers for communicating with the sync API.

## Project Structure

```
src/
└── bookmarkr/
    ├── Commands/         # CLI command definitions and handlers
    ├── Models/           # EF Core entities and data models
    ├── Persistence/      # DbContext and repository implementation
    ├── Service/          # Business logic layer
    ├── ServiceAgent/     # Clients for external services
    ├── appsettings.json  # Application configuration
    └── Program.cs        # Application entry point and DI setup
tests/
└── bookmarkr.Tests/      # Unit tests for the application
```
