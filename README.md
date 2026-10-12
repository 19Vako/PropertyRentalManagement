# Property Rental Management System

ASP.NET Core MVC and Razor application targeting **.NET 10**, organized into *Domain*, *Infrastructure*, and *Web* projects. Identity, role-aware MVC endpoints, automated migrations, and intelligent test data seeding via **Bogus** are fully configured.

## Prerequisites

- **.NET 10 SDK**
- **SQL Server**, **SQL Server Express**, or **Azure SQL Edge** (Docker ARM64)

---

## 1. Configuration & User Secrets

The application manages sensitive configurations and database connection strings using **.NET User Secrets** to prevent committing credentials to version control.

Initialize user secrets for the web project, enable the development seeding flag, and configure your local connection string (replace `<your-secure-password>` with your actual database password):

```sh
cd src/PropertyRental.Web
dotnet user-secrets init
dotnet user-secrets set "DevelopmentSeed:RunOnStartup" "true"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=PropertyRentalManagement;User Id=sa;Password=<your-secure-password>;TrustServerCertificate=True;MultipleActiveResultSets=true"
cd ../..
```

> **Note:** Alternatively, you can set `ConnectionStrings__DefaultConnection` via environment variables.

---

## 2. Database Migrations

If you are setting up the database from scratch or applying new schema updates, make sure the **Entity Framework Core CLI** tools are installed globally:

```sh
dotnet tool install --global dotnet-ef
```

Create and apply migrations to your database:

```sh
dotnet ef migrations add InitialIdentity -p src/PropertyRental.Infrastructure -s src/PropertyRental.Web
dotnet ef database update -p src/PropertyRental.Infrastructure -s src/PropertyRental.Web
```

---

## 3. Build and Run

When starting the application in **Development** mode (`ASPNETCORE_ENVIRONMENT=Development`), the system automatically runs the database initializer (`DevelopmentDatabaseInitializer`):
1. Applies pending **EF Core** migrations.
2. Seeds Identity roles (`Applicant`, `Property Manager`) and the default demo manager account (`manager.demo@example.test`).
3. Executes **Bogus** data seeding (`BogusDataSeeder`) to populate properties, units, and test applications across all statuses.

From the solution root, run the project with:

```sh
dotnet restore
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/PropertyRental.Web
```

---

## 4. Default Access & Testing

| Role / Account Type | Email / Identifier | Password / Access |
| :--- | :--- | :--- |
| **Property Manager** | `manager.demo@example.test` | `ManagerDemo!2026` |
| **Applicant** | *Custom or Seeded* | Register via `/Identity/Account/Register` |

- **Login Page URL:** `/Identity/Account/Login`