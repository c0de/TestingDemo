# Entity Framework Migration Helper Script
# This script provides convenient commands for working with EF migrations

param(
    [Parameter(Position=0)]
    [string]$Command,
    
    [Parameter(Position=1)]
    [string]$Name,
    
    [string]$ConnectionString = "",
    
    [switch]$Help
)

$ProjectPath = "TestingDemo.Entities.Migrations"

function Show-Help {
    Write-Host "Entity Framework Migration Helper" -ForegroundColor Green
    Write-Host "Usage: .\migrate.ps1 <command> [name] [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    Write-Host "  add <name>        - Add a new migration"
    Write-Host "  update [name]     - Update database (to specific migration if name provided)"
    Write-Host "  list              - List all migrations"
    Write-Host "  remove            - Remove last migration (if not applied)"
    Write-Host "  script            - Generate SQL script"
    Write-Host "  drop              - Drop database"
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "  -ConnectionString - Override default connection string"
    Write-Host "  -Help            - Show this help"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\migrate.ps1 add InitialCreate"
    Write-Host "  .\migrate.ps1 update"
    Write-Host "  .\migrate.ps1 list"
    Write-Host '  .\migrate.ps1 add MyMigration -ConnectionString "Server=localhost;Database=MyDb;..."'
}

function Invoke-EfCommand {
    param([string]$EfArgs)
    
    $fullCommand = "dotnet ef $EfArgs --project $ProjectPath"
    
    if ($ConnectionString) {
        $fullCommand += " -- --connection-string `"$ConnectionString`""
    }
    
    Write-Host "Executing: $fullCommand" -ForegroundColor Gray
    Invoke-Expression $fullCommand
}

if ($Help) {
    Show-Help
    exit 0
}

switch ($Command.ToLower()) {
    "add" {
        if (-not $Name) {
            Write-Host "Error: Migration name required for 'add' command" -ForegroundColor Red
            Write-Host "Usage: .\migrate.ps1 add <MigrationName>" -ForegroundColor Yellow
            exit 1
        }
        Invoke-EfCommand "migrations add $Name"
    }
    
    "update" {
        if ($Name) {
            Invoke-EfCommand "database update $Name"
        } else {
            Invoke-EfCommand "database update"
        }
    }
    
    "list" {
        Invoke-EfCommand "migrations list"
    }
    
    "remove" {
        Invoke-EfCommand "migrations remove"
    }
    
    "script" {
        Invoke-EfCommand "migrations script"
    }
    
    "drop" {
        Write-Host "Warning: This will delete all data in the database!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure you want to drop the database? (y/N)"
        if ($confirm -eq 'y' -or $confirm -eq 'Y') {
            Invoke-EfCommand "database drop --force"
        } else {
            Write-Host "Operation cancelled."
        }
    }
    
    default {
        if ($Command) {
            Write-Host "Unknown command: $Command" -ForegroundColor Red
        }
        Show-Help
        if ($Command) { exit 1 }
    }
}
