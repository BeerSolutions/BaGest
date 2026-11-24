# BaGest's SQL Server Database Provider

This project contains BaGest's Microsoft SQL Server database provider.

## Migrations

Check for pending changes with:
```
dotnet ef migrations has-pending-model-changes --context SqlServerContext --startup-project ..\BaGest\BaGest.csproj
```

Add a migration with:

```
dotnet ef migrations add MigrationName --context SqlServerContext --output-dir Migrations --startup-project ..\BaGest\BaGest.csproj
```

Apply the migration to your database with:

```
dotnet ef database update --context SqlServerContext
```
