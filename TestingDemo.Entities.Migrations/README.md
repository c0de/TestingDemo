# Entity Framework Migrations for TestingDemo

This project contains Entity Framework Code First migrations for the TestingDemo application. The DbContext and entities are defined in the `TestingDemo.Entities` project, while migrations, stored procedures, functions, and views are managed here.

## Project Structure

```
TestingDemo.Entities.Migrations/
??? Migrations/              # EF Core migration files (auto-generated)
??? StoredProcedures/        # SQL files for stored procedures
??? Functions/               # SQL files for functions  
??? Views/                   # SQL files for views
??? MigrationDbContextFactory.cs  # Design-time factory for migrations
??? SyncService.cs           # Service for syncing SQL objects
??? DbContextExtensions.cs   # Extension methods for DbContext
??? DatabaseObjectSyncResult.cs  # Result classes for sync operations
```

## Working with Migrations

### Prerequisites
Make sure you have the Entity Framework Core tools installed:
```bash
dotnet tool install --global dotnet-ef
```

### Adding a New Migration
Navigate to the solution root and run:
```bash
dotnet ef migrations add <MigrationName> --project TestingDemo.Entities.Migrations --startup-project <YourStartupProject>
```

Example:
```bash
dotnet ef migrations add InitialCreate --project TestingDemo.Entities.Migrations --startup-project TestingDemo.Api
```

### Updating the Database
```bash
dotnet ef database update --project TestingDemo.Entities.Migrations --startup-project <YourStartupProject>
```

### Removing the Last Migration
```bash
dotnet ef migrations remove --project TestingDemo.Entities.Migrations --startup-project <YourStartupProject>
```

### Listing Migrations
```bash
dotnet ef migrations list --project TestingDemo.Entities.Migrations --startup-project <YourStartupProject>
```

## Configuration in Startup/Program.cs

When configuring your DbContext in your application (API, Web, etc.), use the helper method:

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<DemoDbContext>(options =>
{
    DemoDbContext.ConfigureForSqlServer(
        new DbContextOptionsBuilder<DemoDbContext>(), 
        connectionString
    );
});
```

## SQL Objects (Stored Procedures, Functions, Views)

This project also manages SQL objects through embedded resources:

- **StoredProcedures/**: Contains `.sql` files for stored procedures
- **Functions/**: Contains `.sql` files for functions
- **Views/**: Contains `.sql` files for views

Use the `SyncService` to synchronize these objects with your database:

```csharp
var syncService = new SyncService(logger);
await syncService.SyncSqlObjectsAsync(dbContext);
```

## Connection String

The default connection string used for migrations is:
```
Server=localhost; Integrated Security=True; Encrypt=True; TrustServerCertificate=True; Database=TestDatabase;
```

You can override this by passing it as a command line argument to the migration commands or by modifying the `MigrationDbContextFactory.cs` file.

## Troubleshooting

### "Unable to create an object of type 'DemoDbContext'"
This error typically occurs when EF Core cannot find a design-time factory. Make sure:
1. The `MigrationDbContextFactory` is in the migrations project
2. You're specifying the correct `--project` parameter
3. The connection string is valid

### Migrations appear in wrong project
Ensure you're using the `--project TestingDemo.Entities.Migrations` parameter in your EF Core commands.
