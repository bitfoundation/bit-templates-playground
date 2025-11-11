# Stage 1: Entity Framework Core

Welcome to the interactive Getting Started guide! This document will walk you through the Entity Framework Core architecture in this Bit.TemplatePlayground project.

---

## Overview

Entity Framework Core (EF Core) is the primary data access technology used in this project. It provides an object-relational mapping (ORM) layer that allows you to work with databases using .NET objects, eliminating much of the data access code you'd normally need to write.

In this stage, you'll learn about:
- **AppDbContext**: The central database context for server-side data access
- **Entity Models**: How domain entities are defined
- **Nullable Reference Types**: Understanding nullability in entity properties
- **Entity Type Configurations**: Best practices for configuring entity mappings
- **Migrations**: How to manage database schema changes

---

## 1. AppDbContext - The Heart of Data Access

### Location
The main database context is located at:
[`/src/Server/Bit.TemplatePlayground.Server.Api/Data/AppDbContext.cs`](/src/Server/Bit.TemplatePlayground.Server.Api/Data/AppDbContext.cs)

### What is AppDbContext?

`AppDbContext` is the central class that coordinates Entity Framework functionality for your data model. It inherits from `IdentityDbContext`, which provides built-in support for ASP.NET Core Identity (users, roles, authentication).

### Key Features

Here's the `AppDbContext` structure from the actual project:

```csharp
public partial class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options)
{
    // DbSets represent tables in the database
    public virtual DbSet<Category> Categories { get; set; } = default!;
    public virtual DbSet<Product> Products { get; set; } = default!;
    public virtual DbSet<TodoItem> TodoItems { get; set; } = default!;
    // ... and other DbSets

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```
---

## 2. Entity Models - Defining Your Domain

### Location
Entity models are organized by domain in:
[`/src/Server/Bit.TemplatePlayground.Server.Api/Models/`](/src/Server/Bit.TemplatePlayground.Server.Api/Models/)

The folder structure is:
```
Models/
├── Categories/
│   └── Category.cs
├── Products/
│   └── Product.cs
├── Todo/
│   └── TodoItem.cs
├── Identity/
│   └── User.cs, Role.cs, etc.
└── ... other domains
```

### Example: Category Entity

Let's examine the `Category` entity from the project:

**File:** [`/src/Server/Bit.TemplatePlayground.Server.Api/Models/Categories/Category.cs`](/src/Server/Bit.TemplatePlayground.Server.Api/Models/Categories/Category.cs)

```csharp
using Bit.TemplatePlayground.Server.Api.Models.Products;

namespace Bit.TemplatePlayground.Server.Api.Models.Categories;

public partial class Category
{
    public Guid Id { get; set; }

    [Required, MaxLength(64)]
    public string? Name { get; set; }

    public string? Color { get; set; }

    public byte[] ConcurrencyStamp { get; set; } = [];

    public IList<Product> Products { get; set; } = [];
}
```

### **ConcurrencyStamp**
```csharp
public byte[] ConcurrencyStamp { get; set; } = [];
```
- **Critical for optimistic concurrency control**
- Configured as a **row version** in SQL Server
- Automatically prevents lost updates when multiple users edit the same record
- You **must** include this in all entities that will be updated

---

### Understanding Nullable Reference Types

This project uses C# nullable reference types to improve code safety. Understanding nullability patterns in entity properties is crucial:

#### String Properties with [Required]

```csharp
[Required, MaxLength(64)]
public string? Name { get; set; }
```

**Why is it `string?` (nullable) despite the `[Required]` attribute?**

- EF Core sets properties to `null` **before validation occurs**
- The `[Required]` attribute is a **validation rule**, not a nullability constraint
- During entity instantiation, the property starts as `null`
- Only after validation does EF Core enforce the requirement
- Using `string?` prevents compiler warnings while maintaining validation

#### Navigation Property Patterns

The project follows specific nullability patterns for relationships:

##### The "One" Side (Single Entity Reference)

```csharp
[ForeignKey(nameof(CategoryId))]
public Category? Category { get; set; }

public Guid CategoryId { get; set; }
```

**Pattern: Nullable with `?`**

- **Why nullable?** The related entity might not be loaded from the database
- EF Core uses "lazy loading" or "explicit loading" - related entities aren't automatically fetched
- Example: When you query `Products`, the `Category` property is `null` unless you explicitly include it:
  ```csharp
  // Category will be null
  var product = await dbContext.Products.FirstAsync();
  
  // Category will be loaded
  var product = await dbContext.Products.Include(p => p.Category).FirstAsync();
  ```

##### The "Many" Side (Collection Navigation Property)

```csharp
public IList<Product> Products { get; set; } = [];
```

**Pattern: Non-nullable and initialized with `= []`**

- **Why non-nullable?** Collections should never be `null` to prevent `NullReferenceException`
- **Why `= []`?** Ensures the collection is always initialized, even if no products exist
- You can safely call `.Add()`, `.Count`, etc., without null checks
- Example:
  ```csharp
  var category = new Category();
  category.Products.Add(new Product()); // ✅ Safe - no null check needed
  ```

#### Nullable Reference Types Summary Table

| Property Type | Nullable? | Example | Reason |
|--------------|-----------|---------|--------|
| **String with [Required]** | ✅ Yes (`string?`) | `public string? Name { get; set; }` | EF Core sets to `null` initially before validation |
| **"One" Side Navigation** | ✅ Yes (`Category?`) | `public Category? Category { get; set; }` | Related entity might not be loaded from database |
| **"Many" Side Collection** | ❌ No (initialized with `= []`) | `public IList<Product> Products { get; set; } = []` | Prevents null reference exceptions; safe to iterate |
| **Value Type (non-nullable)** | ❌ No | `public Guid Id { get; set; }` | Value types have default values (e.g., `Guid.Empty`) |
| **Value Type (nullable)** | ✅ Yes (`DateTimeOffset?`) | `public DateTimeOffset? BirthDate { get; set; }` | Explicitly allows null for optional values |

---

## 3. Entity Type Configurations - The Professional Approach

### Location
Entity configurations are located at:
[`/src/Server/Bit.TemplatePlayground.Server.Api/Data/Configurations/`](/src/Server/Bit.TemplatePlayground.Server.Api/Data/Configurations/)

The folder structure mirrors the Models folder:
```
Configurations/
├── Category/
│   └── CategoryConfiguration.cs
├── Product/
│   └── ProductConfiguration.cs
├── Identity/
│   └── UserConfiguration.cs, RoleConfiguration.cs
└── ... other configurations
```

### Example: CategoryConfiguration

**File:** [`/src/Server/Bit.TemplatePlayground.Server.Api/Data/Configurations/Category/CategoryConfiguration.cs`](/src/Server/Bit.TemplatePlayground.Server.Api/Data/Configurations/Category/CategoryConfiguration.cs)

```csharp
using Bit.TemplatePlayground.Server.Api.Models.Categories;

namespace Bit.TemplatePlayground.Server.Api.Data.Configurations.Identity;

public partial class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Configure unique index on Name
        builder.HasIndex(p => p.Name).IsUnique();

        // Seed initial data
        var defaultConcurrencyStamp = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        builder.HasData(
            new Category { 
                Id = Guid.Parse("31d78bd0-0b4f-4e87-b02f-8f66d4ab2845"), 
                Name = "Ford", 
                Color = "#FFCD56", 
                ConcurrencyStamp = defaultConcurrencyStamp 
            },
            new Category { 
                Id = Guid.Parse("582b8c19-0709-4dae-b7a6-fa0e704dad3c"), 
                Name = "Nissan", 
                Color = "#FF6384", 
                ConcurrencyStamp = defaultConcurrencyStamp 
            }
        );
    }
}
```

### Understanding the Configuration

#### 1. **Unique Indexes**
```csharp
builder.HasIndex(p => p.Name).IsUnique();
```
- Creates a unique index on the `Name` column
- Ensures no two categories can have the same name
- Database will enforce this constraint at the SQL level

#### 2. **Seed Data with HasData()**
```csharp
builder.HasData(
    new Category { Id = Guid.Parse("..."), Name = "Ford", ... }
);
```
Pre-populates the database with initial data, data is inserted when the database is created

### How Configurations Are Applied

In `AppDbContext.OnModelCreating()`:
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

This single line:
- **Scans** the entire assembly for classes implementing `IEntityTypeConfiguration<T>`
- **Automatically applies** all configurations
- You don't need to manually register each configuration class

---

## 4. Migrations (Optional for Server-Side)

### Important Note About Migrations

**EF Core migrations are NOT mandatory** in this project, especially for:
- Test projects
- Rapid prototyping scenarios
- Development environments where the database can be easily recreated

### Default Approach: EnsureCreatedAsync()

By default, the project uses `Database.EnsureCreatedAsync()` which **automatically creates** the database schema based on your entities without requiring migrations:

**File:** [`/src/Server/Bit.TemplatePlayground.Server.Api/Program.cs`](/src/Server/Bit.TemplatePlayground.Server.Api/Program.cs)
```csharp
if (builder.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync(); // Automatically creates schema
}
```

### When to Use Migrations?

You should **switch to migrations** when:
- Deploying to **production environments**
- You need to **preserve existing data** during schema changes
- You want **version control** for your database schema
- You're working in a **team environment** where schema changes need to be tracked

### How to Switch to Migrations

If you decide to use migrations, follow these steps:

#### Step 1: Replace EnsureCreatedAsync() with MigrateAsync()

Replace `EnsureCreatedAsync()` with `MigrateAsync()` in these 3 files:
1. [`/src/Server/Bit.TemplatePlayground.Server.Api/Program.cs`](/src/Server/Bit.TemplatePlayground.Server.Api/Program.cs)
2. [`/src/Server/Bit.TemplatePlayground.Server.Web/Program.cs`](/src/Server/Bit.TemplatePlayground.Server.Web/Program.cs)
3. [`/src/Tests/TestsInitializer.cs`](/src/Tests/TestsInitializer.cs)

**Before:**
```csharp
await dbContext.Database.EnsureCreatedAsync();
```

**After:**
```csharp
await dbContext.Database.MigrateAsync();
```

#### Step 2: Delete Existing Database (If Applicable)

**Important:** If you've already run the project with `EnsureCreatedAsync()`, you **must delete the existing database** before switching to migrations. 

- `EnsureCreatedAsync()` and `MigrateAsync()` cannot be mixed
- Your database will be recreated with the initial migration

#### Step 3: Create Your First Migration

Open a terminal in the `Bit.TemplatePlayground.Server.Api` project directory and run:

```bash
dotnet ef migrations add Initial --output-dir Data/Migrations --verbose
```

This creates migration files in the `/Data/Migrations/` folder.

#### Step 4: Apply the Migration

The migration will be **automatically applied** when the application starts (thanks to `MigrateAsync()`).

**Note:** You do **NOT** need to run `dotnet ef database update` or `Update-Database` manually. The `MigrateAsync()` call in the application startup code handles this automatically.

### Adding Future Migrations

When you modify entities or configurations, create a new migration:

```bash
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations --verbose
```

---
