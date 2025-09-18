# Entity Framework Migrations

This project contains the Entity Framework migrations for the TestingDemo application.

## Project Structure

- **TestingDemo.Entities**: Contains the `DemoDbContext` and entity models
- **TestingDemo.Entities.Migrations**: Contains migrations, design-time factory, and database sync utilities

## Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB, SQL Server Express, or full SQL Server)
- Entity Framework Core tools

## Install EF Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

## Working with Migrations

### Creating a New Migration

Navigate to the solution root and run:

```bash
# Create a migration
dotnet ef migrations add <MigrationName> --project TestingDemo.Entities.Migrations

# Example
dotnet ef migrations add InitialCreate --project TestingDemo.Entities.Migrations
```

### Applying Migrations

```bash
# Update database to latest migration
dotnet ef database update --project TestingDemo.Entities.Migrations

# Update to specific migration
dotnet ef database update <MigrationName> --project TestingDemo.Entities.Migrations
```

### Other Useful Commands

```bash
# List all migrations
dotnet ef migrations list --project TestingDemo.Entities.Migrations

# Remove last migration (if not applied to database)
dotnet ef migrations remove --project TestingDemo.Entities.Migrations

# Generate SQL script from migrations
dotnet ef migrations script --project TestingDemo.Entities.Migrations

# Drop database
dotnet ef database drop --project TestingDemo.Entities.Migrations
```

### Using the PowerShell Helper Script

For convenience, you can use the provided `migrate.ps1` script:

```bash
# Create a migration
.\migrate.ps1 add MyMigrationName

# Update database
.\migrate.ps1 update

# List migrations
.\migrate.ps1 list

# Remove last migration
.\migrate.ps1 remove

# Drop database (with confirmation)
.\migrate.ps1 drop
```

## Connection String Configuration

The `DemoDbContextFactory` looks for connection strings in this order:

1. **Command Line**: `--connection-string "your-connection-string"`
2. **Environment Variable**: `MIGRATION_CONNECTION_STRING`
3. **Default**: `Server=localhost;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Database=TestingDemoDev;`

### Examples

```bash
# Using command line
dotnet ef migrations add MyMigration --project TestingDemo.Entities.Migrations -- --connection-string "Server=localhost;Database=MyTestDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"

# Using environment variable
set MIGRATION_CONNECTION_STRING=Server=localhost;Database=MyTestDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;
dotnet ef migrations add MyMigration --project TestingDemo.Entities.Migrations

# Using PowerShell script with custom connection string
.\migrate.ps1 add MyMigration -ConnectionString "Server=localhost;Database=MyTestDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
```

## Troubleshooting

### Common Issues

1. **"Unable to create an object of type 'DemoDbContext'"**
   - Ensure the `DemoDbContextFactory` is in the migrations project
   - Check that the connection string is valid

2. **"The Entity Framework tools version is older than that of the runtime"**
   - Update EF tools: `dotnet tool update --global dotnet-ef`

3. **Connection issues**
   - Verify SQL Server is running
   - Check connection string format
   - Ensure database permissions

### Verify Setup

Test your setup by running:

```bash
dotnet ef migrations list --project TestingDemo.Entities.Migrations
```

If this runs without errors, your migrations setup is working correctly.

## Integration with Your Application

Your application should use the `DemoDbContext.ConfigureForSqlServer` method to ensure consistency:

```csharp
// In your application startup
services.AddDbContext<DemoDbContext>(options =>
{
    DemoDbContext.ConfigureForSqlServer(options, connectionString);
});
