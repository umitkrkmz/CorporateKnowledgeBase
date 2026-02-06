namespace CorporateKnowledgeBase.Infrastructure.Data;

using CorporateKnowledgeBase.Domain.Entities;
using CorporateKnowledgeBase.Domain.Enums;
using CorporateKnowledgeBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class SeedData
{
    // ╔═══════════════════════════════════════════════════════════════╗
    //  ADMIN SEED — Runs on every startup
    //  Creates: Roles + Admin account (minimum required for login)
    // ╚═══════════════════════════════════════════════════════════════╝
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // ── Roles ──
        string[] roles = ["Admin", "Editor", "Member"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Default Admin Account ──
        if (!await context.Departments.AnyAsync(d => d.Name == "Engineering"))
        {
            context.Departments.Add(new Department
            {
                Name = "Engineering",
                Description = "Software engineering and development team",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        var engineeringDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "admin@corp.com", Email = "admin@corp.com",
            FullName = "System Administrator", Department = "Engineering",
            DepartmentId = engineeringDept?.Id, JobTitle = "System Admin",
            IsApproved = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
        }, "Admin123!", "Admin");
    }

    // ╔═══════════════════════════════════════════════════════════════╗
    //  DEMO SEED — Triggered manually by Admin
    //  Creates: Departments, Users, Categories, Tags, Documents,
    //           Blog Posts, Announcements, Comments, FAQ Items
    //  Safe to run multiple times (idempotent checks)
    // ╚═══════════════════════════════════════════════════════════════╝
    public static async Task<SeedResult> SeedDemoDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = new SeedResult();

        // Check if demo data already exists
        if (await context.Categories.AnyAsync() &&
            await context.Tags.AnyAsync() &&
            await context.Documents.AnyAsync())
        {
            result.Message = "Demo verileri zaten mevcut. Tekrar yüklemek için önce mevcut verileri temizleyin.";
            result.AlreadySeeded = true;
            return result;
        }

        // ═══════════════════════════════════════════
        //  DEPARTMENTS
        // ═══════════════════════════════════════════
        if (!await context.Departments.AnyAsync(d => d.Name == "DevOps"))
        {
            var newDepts = new List<Department>();
            if (!await context.Departments.AnyAsync(d => d.Name == "DevOps"))
                newDepts.Add(new Department { Name = "DevOps", Description = "Infrastructure, CI/CD and cloud operations team", CreatedAt = DateTime.UtcNow });
            if (!await context.Departments.AnyAsync(d => d.Name == "QA"))
                newDepts.Add(new Department { Name = "QA", Description = "Quality assurance and testing team", CreatedAt = DateTime.UtcNow });
            if (!await context.Departments.AnyAsync(d => d.Name == "Product"))
                newDepts.Add(new Department { Name = "Product", Description = "Product management and design team", CreatedAt = DateTime.UtcNow });
            if (!await context.Departments.AnyAsync(d => d.Name == "HR"))
                newDepts.Add(new Department { Name = "HR", Description = "Human resources department", CreatedAt = DateTime.UtcNow });

            if (newDepts.Count > 0)
            {
                context.Departments.AddRange(newDepts);
                await context.SaveChangesAsync();
                result.DepartmentsCreated = newDepts.Count;
            }
        }

        var engineeringDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
        var devOpsDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "DevOps");
        var qaDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "QA");

        // ═══════════════════════════════════════════
        //  USERS  (5 demo accounts + 1 pending)
        // ═══════════════════════════════════════════
        //
        //  ┌──────────────────┬──────────────┬────────┬─────────────┐
        //  │ Email            │ Password     │ Role   │ Department  │
        //  ├──────────────────┼──────────────┼────────┼─────────────┤
        //  │ editor@corp.com  │ Editor123!   │ Editor │ Engineering │
        //  │ member@corp.com  │ Member123!   │ Member │ Engineering │
        //  │ devops@corp.com  │ DevOps123!   │ Editor │ DevOps      │
        //  │ qa@corp.com      │ Qa1234!      │ Member │ QA          │
        //  │ pending@corp.com │ Pending123!  │ Member │ (none)      │
        //  └──────────────────┴──────────────┴────────┴─────────────┘

        var admin = await userManager.FindByEmailAsync("admin@corp.com");

        var editor = await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "editor@corp.com", Email = "editor@corp.com",
            FullName = "Jane Editor", Department = "Engineering",
            DepartmentId = engineeringDept?.Id, JobTitle = "Senior Developer",
            IsApproved = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
        }, "Editor123!", "Editor");

        var member = await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "member@corp.com", Email = "member@corp.com",
            FullName = "John Member", Department = "Engineering",
            DepartmentId = engineeringDept?.Id, JobTitle = "Junior Developer",
            IsApproved = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
        }, "Member123!", "Member");

        var devopsUser = await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "devops@corp.com", Email = "devops@corp.com",
            FullName = "Alex DevOps", Department = "DevOps",
            DepartmentId = devOpsDept?.Id, JobTitle = "DevOps Engineer",
            IsApproved = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
        }, "DevOps123!", "Editor");

        var qaUser = await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "qa@corp.com", Email = "qa@corp.com",
            FullName = "Sarah QA", Department = "QA",
            DepartmentId = qaDept?.Id, JobTitle = "QA Analyst",
            IsApproved = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
        }, "Qa1234!", "Member");

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            UserName = "pending@corp.com", Email = "pending@corp.com",
            FullName = "Peter Pending", Department = "Product",
            JobTitle = "Intern", IsApproved = false, EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        }, "Pending123!", "Member");

        result.UsersCreated = 5;

        // ═══════════════════════════════════════════
        //  CATEGORIES
        // ═══════════════════════════════════════════
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Backend", Description = "Server-side development, APIs, business logic", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Frontend", Description = "Client-side development, UI/UX, CSS frameworks", CreatedAt = DateTime.UtcNow },
                new Category { Name = "DevOps", Description = "CI/CD, containers, cloud infrastructure", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Database", Description = "Database design, SQL, ORM, migrations", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Architecture", Description = "Software architecture, design patterns, best practices", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Testing", Description = "Unit testing, integration testing, QA strategies", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            result.CategoriesCreated = 6;
        }

        // ═══════════════════════════════════════════
        //  TAGS
        // ═══════════════════════════════════════════
        if (!await context.Tags.AnyAsync())
        {
            context.Tags.AddRange(
                new Tag { Name = "csharp", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "dotnet", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "ef-core", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "clean-architecture", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "cqrs", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "mediatr", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "docker", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "ci-cd", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "github-actions", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "sql-server", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "performance", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "react", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "tailwind", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "css", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "unit-testing", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "integration-testing", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "design-patterns", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "repository-pattern", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "dependency-injection", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "aspnet-core", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "blazor", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "kubernetes", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "rest-api", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "security", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            result.TagsCreated = 24;
        }

        // ═══════════════════════════════════════════
        //  SAMPLE CONTENT
        // ═══════════════════════════════════════════
        if (!await context.Documents.AnyAsync())
        {
            var catBackend = await context.Categories.FirstAsync(c => c.Name == "Backend");
            var catFrontend = await context.Categories.FirstAsync(c => c.Name == "Frontend");
            var catDevOps = await context.Categories.FirstAsync(c => c.Name == "DevOps");
            var catDatabase = await context.Categories.FirstAsync(c => c.Name == "Database");
            var catArch = await context.Categories.FirstAsync(c => c.Name == "Architecture");
            var catTesting = await context.Categories.FirstAsync(c => c.Name == "Testing");

            var tagCsharp = await context.Tags.FirstAsync(t => t.Name == "csharp");
            var tagDotnet = await context.Tags.FirstAsync(t => t.Name == "dotnet");
            var tagEfCore = await context.Tags.FirstAsync(t => t.Name == "ef-core");
            var tagCleanArch = await context.Tags.FirstAsync(t => t.Name == "clean-architecture");
            var tagCqrs = await context.Tags.FirstAsync(t => t.Name == "cqrs");
            var tagMediatr = await context.Tags.FirstAsync(t => t.Name == "mediatr");
            var tagDocker = await context.Tags.FirstAsync(t => t.Name == "docker");
            var tagCiCd = await context.Tags.FirstAsync(t => t.Name == "ci-cd");
            var tagGhActions = await context.Tags.FirstAsync(t => t.Name == "github-actions");
            var tagSqlServer = await context.Tags.FirstAsync(t => t.Name == "sql-server");
            var tagPerf = await context.Tags.FirstAsync(t => t.Name == "performance");
            var tagReact = await context.Tags.FirstAsync(t => t.Name == "react");
            var tagTailwind = await context.Tags.FirstAsync(t => t.Name == "tailwind");
            var tagCss = await context.Tags.FirstAsync(t => t.Name == "css");
            var tagUnitTest = await context.Tags.FirstAsync(t => t.Name == "unit-testing");
            var tagIntTest = await context.Tags.FirstAsync(t => t.Name == "integration-testing");
            var tagDesignPat = await context.Tags.FirstAsync(t => t.Name == "design-patterns");
            var tagRepo = await context.Tags.FirstAsync(t => t.Name == "repository-pattern");
            var tagDI = await context.Tags.FirstAsync(t => t.Name == "dependency-injection");
            var tagAspnet = await context.Tags.FirstAsync(t => t.Name == "aspnet-core");
            var tagBlazor = await context.Tags.FirstAsync(t => t.Name == "blazor");
            var tagK8s = await context.Tags.FirstAsync(t => t.Name == "kubernetes");
            var tagRestApi = await context.Tags.FirstAsync(t => t.Name == "rest-api");
            var tagSecurity = await context.Tags.FirstAsync(t => t.Name == "security");

            // ────────────────────────────────
            //  DOCUMENTS  (8 total: 6 published, 1 draft, 1 pending)
            // ────────────────────────────────

            var doc1 = new TechnicalDocument
            {
                Title = "Clean Architecture Guide",
                Content = "# Clean Architecture\n\n" +
                    "Clean Architecture separates concerns into concentric layers, each with a clear responsibility.\n\n" +
                    "## Domain Layer\n" +
                    "Contains entities, value objects, and business rules. This layer has **zero** external dependencies.\n\n" +
                    "## Application Layer\n" +
                    "Defines use cases (commands & queries) and interfaces. Depends only on Domain.\n\n" +
                    "## Infrastructure Layer\n" +
                    "Implements persistence (EF Core), external services, and identity. See [[Entity Framework Core Performance Tips]] for optimization.\n\n" +
                    "## Web / Presentation Layer\n" +
                    "Controllers, views, and middleware. Depends on Application layer via dependency injection.\n\n" +
                    "## Key Principles\n" +
                    "- Dependencies point **inward** (outer layers depend on inner layers)\n" +
                    "- Business rules are isolated from frameworks\n" +
                    "- Infrastructure is swappable without touching domain logic\n\n" +
                    "> **See also:** [[CQRS with MediatR]] and [[Repository Pattern Best Practices]]",
                AuthorId = admin!.Id,
                CategoryId = catArch.Id,
                Status = ContentStatus.Published,
                ViewCount = 142,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                Tags = [tagCleanArch, tagCsharp, tagDesignPat, tagDI]
            };

            var doc2 = new TechnicalDocument
            {
                Title = "CQRS with MediatR",
                Content = "# CQRS with MediatR\n\n" +
                    "Command Query Responsibility Segregation (CQRS) separates read and write operations into distinct models.\n\n" +
                    "## Why CQRS?\n" +
                    "Traditional CRUD architectures use the same model for reads and writes. As complexity grows, this creates:\n" +
                    "- Bloated DTOs with unused fields\n" +
                    "- Complex validation logic mixed with queries\n" +
                    "- Difficult to optimize reads independently from writes\n\n" +
                    "## Commands (Write Side)\n" +
                    "Commands modify state and return minimal data:\n" +
                    "```csharp\n" +
                    "public record CreateDocumentCommand(string Title, string Content, string AuthorId) : IRequest<int>;\n\n" +
                    "public class CreateDocumentHandler(IApplicationDbContext context) : IRequestHandler<CreateDocumentCommand, int>\n" +
                    "{\n" +
                    "    public async Task<int> Handle(CreateDocumentCommand request, CancellationToken ct)\n" +
                    "    {\n" +
                    "        var doc = new TechnicalDocument { Title = request.Title, Content = request.Content };\n" +
                    "        context.Documents.Add(doc);\n" +
                    "        await context.SaveChangesAsync(ct);\n" +
                    "        return doc.Id;\n" +
                    "    }\n" +
                    "}\n```\n\n" +
                    "## Queries (Read Side)\n" +
                    "Queries return data without side effects:\n" +
                    "```csharp\n" +
                    "public record GetAllDocumentsQuery() : IRequest<List<DocumentDto>>;\n```\n\n" +
                    "## Integration with [[Clean Architecture Guide]]\n" +
                    "CQRS fits naturally into Clean Architecture — commands and queries live in the Application layer.",
                AuthorId = editor!.Id,
                CategoryId = catArch.Id,
                Status = ContentStatus.Published,
                ViewCount = 98,
                CreatedAt = DateTime.UtcNow.AddDays(-38),
                Tags = [tagCqrs, tagMediatr, tagCsharp, tagCleanArch, tagDesignPat]
            };

            var doc3 = new TechnicalDocument
            {
                Title = "Repository Pattern Best Practices",
                Content = "# Repository Pattern Best Practices\n\n" +
                    "The Repository pattern provides an abstraction over data access, making your code testable and maintainable.\n\n" +
                    "## Generic Repository Interface\n" +
                    "```csharp\n" +
                    "public interface IRepository<T> where T : BaseEntity\n" +
                    "{\n" +
                    "    Task<T?> GetByIdAsync(int id);\n" +
                    "    Task<List<T>> GetAllAsync();\n" +
                    "    void Add(T entity);\n" +
                    "    void Remove(T entity);\n" +
                    "}\n```\n\n" +
                    "## Unit of Work\n" +
                    "Combine repositories with Unit of Work for transaction management:\n" +
                    "```csharp\n" +
                    "public interface IUnitOfWork\n" +
                    "{\n" +
                    "    IRepository<TechnicalDocument> Documents { get; }\n" +
                    "    Task<int> SaveChangesAsync(CancellationToken ct = default);\n" +
                    "}\n```\n\n" +
                    "## Best Practices\n" +
                    "1. **Keep repositories focused** — one per aggregate root\n" +
                    "2. **Use Specification pattern** for complex queries\n" +
                    "3. **Don't leak IQueryable** outside the repository\n" +
                    "4. **Prefer explicit methods** over generic ones for complex queries\n\n" +
                    "> For performance tips with EF Core, see [[Entity Framework Core Performance Tips]]",
                AuthorId = editor.Id,
                CategoryId = catBackend.Id,
                Status = ContentStatus.Published,
                ViewCount = 67,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Tags = [tagRepo, tagCsharp, tagDesignPat, tagEfCore]
            };

            var doc4 = new TechnicalDocument
            {
                Title = "Entity Framework Core Performance Tips",
                Content = "# Entity Framework Core — Performance Tips\n\n" +
                    "EF Core is powerful but can become a bottleneck if misused. Here are proven optimization techniques.\n\n" +
                    "## 1. Use AsNoTracking for Read-Only Queries\n" +
                    "```csharp\n" +
                    "var docs = await context.Documents\n" +
                    "    .AsNoTracking()\n" +
                    "    .Where(d => d.Status == ContentStatus.Published)\n" +
                    "    .ToListAsync();\n```\n" +
                    "This skips the change tracker, reducing memory allocation by ~30%.\n\n" +
                    "## 2. Avoid N+1 Queries\n" +
                    "Use `.Include()` for eager loading:\n" +
                    "```csharp\n" +
                    "var docs = await context.Documents\n" +
                    "    .Include(d => d.Tags)\n" +
                    "    .Include(d => d.Category)\n" +
                    "    .ToListAsync();\n```\n\n" +
                    "## 3. Use Projection with Select()\n" +
                    "Fetch only the columns you need:\n" +
                    "```csharp\n" +
                    "var titles = await context.Documents\n" +
                    "    .Select(d => new { d.Id, d.Title })\n" +
                    "    .ToListAsync();\n```\n\n" +
                    "## 4. Pagination\n" +
                    "Always paginate large result sets:\n" +
                    "```csharp\n" +
                    "var page = await context.Documents\n" +
                    "    .OrderByDescending(d => d.CreatedAt)\n" +
                    "    .Skip(pageIndex * pageSize)\n" +
                    "    .Take(pageSize)\n" +
                    "    .ToListAsync();\n```\n\n" +
                    "## 5. Compiled Queries\n" +
                    "For hot paths, use compiled queries to avoid repeated expression tree compilation.\n\n" +
                    "## 6. Batch Operations\n" +
                    "EF Core 10 supports `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for bulk operations.",
                AuthorId = admin.Id,
                CategoryId = catDatabase.Id,
                Status = ContentStatus.Published,
                ViewCount = 113,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                Tags = [tagEfCore, tagSqlServer, tagPerf, tagCsharp, tagDotnet]
            };

            var doc5 = new TechnicalDocument
            {
                Title = "Docker Deployment Guide",
                Content = "# Docker Deployment Guide\n\n" +
                    "This guide covers containerizing .NET applications with Docker.\n\n" +
                    "## Multi-Stage Dockerfile\n" +
                    "```dockerfile\n" +
                    "# Build stage\n" +
                    "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
                    "WORKDIR /src\n" +
                    "COPY *.sln .\n" +
                    "COPY **/*.csproj ./\n" +
                    "RUN dotnet restore\n" +
                    "COPY . .\n" +
                    "RUN dotnet publish -c Release -o /app/publish --no-restore\n\n" +
                    "# Runtime stage\n" +
                    "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime\n" +
                    "WORKDIR /app\n" +
                    "COPY --from=build /app/publish .\n" +
                    "EXPOSE 8080\n" +
                    "ENTRYPOINT [\"dotnet\", \"CorporateKnowledgeBase.Web.dll\"]\n```\n\n" +
                    "## Docker Compose\n" +
                    "```yaml\n" +
                    "services:\n" +
                    "  web:\n" +
                    "    build: .\n" +
                    "    ports: [\"8080:8080\"]\n" +
                    "    depends_on: [db]\n" +
                    "  db:\n" +
                    "    image: mcr.microsoft.com/mssql/server:2022-latest\n" +
                    "    environment:\n" +
                    "      SA_PASSWORD: \"YourStrong!Pass\"\n" +
                    "      ACCEPT_EULA: \"Y\"\n```\n\n" +
                    "## Health Checks\n" +
                    "Add health checks to monitor your container:\n" +
                    "```csharp\n" +
                    "builder.Services.AddHealthChecks()\n" +
                    "    .AddSqlServer(connectionString);\n```\n\n" +
                    "> For CI/CD integration, see the blog post [[CI/CD Pipeline with GitHub Actions]]",
                AuthorId = devopsUser!.Id,
                CategoryId = catDevOps.Id,
                Status = ContentStatus.Published,
                DepartmentId = devOpsDept?.Id,
                ViewCount = 89,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Tags = [tagDocker, tagDotnet, tagCiCd, tagK8s]
            };

            var doc6 = new TechnicalDocument
            {
                Title = "ASP.NET Core Authentication & Authorization",
                Content = "# ASP.NET Core Authentication & Authorization\n\n" +
                    "This document covers Identity-based auth in ASP.NET Core applications.\n\n" +
                    "## Authentication Setup\n" +
                    "```csharp\n" +
                    "builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>\n" +
                    "{\n" +
                    "    options.Password.RequireDigit = true;\n" +
                    "    options.Password.RequiredLength = 6;\n" +
                    "})\n" +
                    ".AddEntityFrameworkStores<ApplicationDbContext>()\n" +
                    ".AddDefaultTokenProviders();\n```\n\n" +
                    "## Role-Based Authorization\n" +
                    "```csharp\n" +
                    "[Authorize(Roles = \"Admin,Editor\")]\n" +
                    "public class AdminController : Controller { }\n```\n\n" +
                    "## Custom Authorization Policies\n" +
                    "```csharp\n" +
                    "builder.Services.AddAuthorization(options =>\n" +
                    "{\n" +
                    "    options.AddPolicy(\"CanEditContent\", policy =>\n" +
                    "        policy.RequireRole(\"Admin\", \"Editor\"));\n" +
                    "});\n```\n\n" +
                    "## Claims-Based Identity\n" +
                    "Add custom claims during sign-in to avoid database lookups on every request.\n\n" +
                    "> See [[Clean Architecture Guide]] for where auth services fit in the architecture.",
                AuthorId = admin.Id,
                CategoryId = catBackend.Id,
                Status = ContentStatus.Published,
                ViewCount = 76,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Tags = [tagAspnet, tagSecurity, tagCsharp, tagDI]
            };

            var doc7 = new TechnicalDocument
            {
                Title = "React Component Patterns",
                Content = "# React Component Patterns\n\n" +
                    "Modern React patterns for building reusable, maintainable components.\n\n" +
                    "## Compound Components\n" +
                    "Allow parent components to communicate with children implicitly:\n" +
                    "```jsx\n" +
                    "<Select onChange={handleChange}>\n" +
                    "  <Select.Option value=\"a\">Option A</Select.Option>\n" +
                    "  <Select.Option value=\"b\">Option B</Select.Option>\n" +
                    "</Select>\n```\n\n" +
                    "## Custom Hooks\n" +
                    "Extract reusable logic into custom hooks:\n" +
                    "```jsx\n" +
                    "function useDebounce(value, delay) {\n" +
                    "  const [debounced, setDebounced] = useState(value);\n" +
                    "  useEffect(() => {\n" +
                    "    const timer = setTimeout(() => setDebounced(value), delay);\n" +
                    "    return () => clearTimeout(timer);\n" +
                    "  }, [value, delay]);\n" +
                    "  return debounced;\n" +
                    "}\n```\n\n" +
                    "## Render Props vs HOCs\n" +
                    "Prefer custom hooks over render props and HOCs for most use cases.",
                AuthorId = member!.Id,
                CategoryId = catFrontend.Id,
                Status = ContentStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Tags = [tagReact, tagDesignPat]
            };

            var doc8 = new TechnicalDocument
            {
                Title = "Unit Testing Best Practices",
                Content = "# Unit Testing Best Practices\n\n" +
                    "Writing effective unit tests is crucial for maintaining code quality.\n\n" +
                    "## AAA Pattern\n" +
                    "Every test should follow **Arrange, Act, Assert**:\n" +
                    "```csharp\n" +
                    "[Fact]\n" +
                    "public async Task CreateDocument_ValidInput_ReturnsDocumentId()\n" +
                    "{\n" +
                    "    // Arrange\n" +
                    "    var command = new CreateDocumentCommand(\"Title\", \"Content\", \"author-id\");\n\n" +
                    "    // Act\n" +
                    "    var result = await _handler.Handle(command, CancellationToken.None);\n\n" +
                    "    // Assert\n" +
                    "    Assert.True(result > 0);\n" +
                    "}\n```\n\n" +
                    "## Naming Convention\n" +
                    "`MethodName_Scenario_ExpectedResult` — makes test intent clear.\n\n" +
                    "## Mocking Dependencies\n" +
                    "Use **Moq** or **NSubstitute** to isolate the system under test:\n" +
                    "```csharp\n" +
                    "var mockContext = new Mock<IApplicationDbContext>();\n" +
                    "mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))\n" +
                    "    .ReturnsAsync(1);\n```\n\n" +
                    "## Code Coverage\n" +
                    "- Aim for **meaningful** coverage, not 100%\n" +
                    "- Focus on business logic, not boilerplate\n" +
                    "- Use mutation testing to validate test quality",
                AuthorId = qaUser!.Id,
                CategoryId = catTesting.Id,
                Status = ContentStatus.PendingReview,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Tags = [tagUnitTest, tagCsharp, tagDotnet]
            };

            context.Documents.AddRange(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8);
            await context.SaveChangesAsync();
            result.DocumentsCreated = 8;

            // ────────────────────────────────
            //  BLOG POSTS  (6 total: 4 published, 1 pending, 1 draft)
            // ────────────────────────────────

            var blog1 = new BlogPost
            {
                Title = "What's New in .NET 10 LTS",
                Content = "<h2>What's New in .NET 10 LTS</h2>" +
                    "<p>.NET 10, the latest Long-Term Support release, brings substantial improvements across the entire stack.</p>" +
                    "<h3>Performance Improvements</h3>" +
                    "<ul>" +
                    "<li><strong>JSON Serialization</strong>: Up to 20% faster with source-generated serializers</li>" +
                    "<li><strong>LINQ</strong>: Optimized enumerable operations with reduced allocations</li>" +
                    "<li><strong>GC</strong>: Improved garbage collection for server workloads</li>" +
                    "</ul>" +
                    "<h3>ASP.NET Core 10</h3>" +
                    "<ul>" +
                    "<li>Enhanced Minimal API parameter binding</li>" +
                    "<li>Improved OpenAPI document generation</li>" +
                    "<li>Better Blazor server-side rendering performance</li>" +
                    "</ul>" +
                    "<h3>Entity Framework Core 10</h3>" +
                    "<ul>" +
                    "<li>Better query translation for complex LINQ expressions</li>" +
                    "<li>Improved batch operation support</li>" +
                    "<li>New migration bundle features</li>" +
                    "</ul>" +
                    "<p>For architecture guidance with .NET 10, check out the [[Clean Architecture Guide]].</p>",
                AuthorId = admin.Id,
                CategoryId = catBackend.Id,
                Status = ContentStatus.Published,
                ViewCount = 234,
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                Tags = [tagDotnet, tagCsharp, tagAspnet, tagEfCore, tagBlazor, tagPerf]
            };

            var blog2 = new BlogPost
            {
                Title = "Why We Switched to Vertical Slice Architecture",
                Content = "<h2>From Layered to Vertical Slice Architecture</h2>" +
                    "<p>After two years with traditional Clean Architecture, our team adopted Vertical Slice Architecture with MediatR. Here's our journey.</p>" +
                    "<h3>The Problem</h3>" +
                    "<p>In our previous architecture, a simple CRUD feature required changes across 5+ files: Entity, DTO, Repository Interface, Repository Implementation, Service, Controller. This led to:</p>" +
                    "<ul>" +
                    "<li>High cognitive overhead for simple features</li>" +
                    "<li>Frequent merge conflicts across layers</li>" +
                    "<li>Abstractions that added complexity without value</li>" +
                    "</ul>" +
                    "<h3>The Solution: Feature Folders + MediatR</h3>" +
                    "<p>Each feature is now self-contained in a single file: Command/Query record + Handler + DTO. Related documentation: [[CQRS with MediatR]].</p>" +
                    "<h3>Results After 6 Months</h3>" +
                    "<ul>" +
                    "<li><strong>40% faster</strong> feature development</li>" +
                    "<li><strong>60% fewer</strong> merge conflicts</li>" +
                    "<li>New team members productive within days, not weeks</li>" +
                    "</ul>",
                AuthorId = editor.Id,
                CategoryId = catArch.Id,
                Status = ContentStatus.Published,
                ViewCount = 156,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Tags = [tagCleanArch, tagCqrs, tagMediatr, tagDesignPat, tagCsharp]
            };

            var blog3 = new BlogPost
            {
                Title = "CI/CD Pipeline with GitHub Actions",
                Content = "<h2>Setting Up a Production-Ready CI/CD Pipeline</h2>" +
                    "<p>Our team recently migrated from Jenkins to GitHub Actions. This post shares our workflow configuration and lessons learned.</p>" +
                    "<h3>Pipeline Stages</h3>" +
                    "<ol>" +
                    "<li><strong>Build</strong>: Restore packages, compile solution</li>" +
                    "<li><strong>Test</strong>: Run unit and integration tests</li>" +
                    "<li><strong>Analyze</strong>: SonarQube code quality scan</li>" +
                    "<li><strong>Deploy to Staging</strong>: Docker build + push + deploy</li>" +
                    "<li><strong>Deploy to Production</strong>: Manual approval gate + rolling deployment</li>" +
                    "</ol>" +
                    "<h3>Key Configuration</h3>" +
                    "<pre><code>name: CI/CD Pipeline\non:\n  push:\n    branches: [main]\n  pull_request:\n    branches: [main]\n\njobs:\n  build-and-test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '10.0.x'\n      - run: dotnet build --configuration Release\n      - run: dotnet test --no-build</code></pre>" +
                    "<h3>Tips</h3>" +
                    "<ul>" +
                    "<li>Cache NuGet packages to speed up builds</li>" +
                    "<li>Use environment-specific secrets for database connections</li>" +
                    "<li>Set up Slack notifications for failed deployments</li>" +
                    "</ul>" +
                    "<p>For containerization details, see [[Docker Deployment Guide]].</p>",
                AuthorId = devopsUser.Id,
                CategoryId = catDevOps.Id,
                Status = ContentStatus.Published,
                DepartmentId = devOpsDept?.Id,
                ViewCount = 112,
                CreatedAt = DateTime.UtcNow.AddDays(-22),
                Tags = [tagCiCd, tagGhActions, tagDocker, tagDotnet]
            };

            var blog4 = new BlogPost
            {
                Title = "Getting Started with Tailwind CSS in ASP.NET Core",
                Content = "<h2>Tailwind CSS in .NET Projects</h2>" +
                    "<p>Tailwind CSS is a utility-first CSS framework that accelerates UI development. Here's how we integrated it into our ASP.NET Core MVC project.</p>" +
                    "<h3>Installation</h3>" +
                    "<pre><code>npm install -D tailwindcss postcss autoprefixer\nnpx tailwindcss init</code></pre>" +
                    "<h3>Configuration</h3>" +
                    "<p>Update <code>tailwind.config.js</code> to scan your Razor views:</p>" +
                    "<pre><code>module.exports = {\n  content: ['./Views/**/*.cshtml', './wwwroot/js/**/*.js'],\n  theme: { extend: {} },\n  plugins: [],\n}</code></pre>" +
                    "<h3>Why Tailwind Over Bootstrap?</h3>" +
                    "<ul>" +
                    "<li><strong>No naming debates</strong> — utility classes describe what they do</li>" +
                    "<li><strong>Smaller bundle</strong> — PurgeCSS removes unused styles (~10KB gzipped)</li>" +
                    "<li><strong>Design consistency</strong> — built-in design tokens for spacing, colors, typography</li>" +
                    "</ul>" +
                    "<h3>When to Stick with Bootstrap</h3>" +
                    "<p>Bootstrap is still great for rapid prototyping and admin panels where custom design isn't a priority.</p>",
                AuthorId = member.Id,
                CategoryId = catFrontend.Id,
                Status = ContentStatus.Published,
                ViewCount = 87,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                Tags = [tagTailwind, tagCss, tagAspnet]
            };

            var blog5 = new BlogPost
            {
                Title = "Integration Testing with TestContainers for .NET",
                Content = "<h2>Real Database Testing with TestContainers</h2>" +
                    "<p>In-memory databases hide real SQL issues. TestContainers solves this by running actual database engines in Docker during tests.</p>" +
                    "<h3>Setup</h3>" +
                    "<pre><code>dotnet add package Testcontainers.MsSql</code></pre>" +
                    "<h3>Test Fixture</h3>" +
                    "<pre><code>public class DatabaseFixture : IAsyncLifetime\n{\n    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();\n\n    public string ConnectionString => _container.GetConnectionString();\n\n    public Task InitializeAsync() => _container.StartAsync();\n    public Task DisposeAsync() => _container.DisposeAsync().AsTask();\n}</code></pre>" +
                    "<h3>Benefits Over InMemory Provider</h3>" +
                    "<ul>" +
                    "<li>Tests run against real SQL Server — catches query translation issues</li>" +
                    "<li>Each test run gets a fresh, isolated database</li>" +
                    "<li>No test pollution between parallel test classes</li>" +
                    "<li>Validates migrations and seed data</li>" +
                    "</ul>" +
                    "<p>For unit testing guidelines, see [[Unit Testing Best Practices]].</p>",
                AuthorId = qaUser.Id,
                CategoryId = catTesting.Id,
                Status = ContentStatus.PendingReview,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Tags = [tagIntTest, tagDocker, tagEfCore, tagSqlServer]
            };

            var blog6 = new BlogPost
            {
                Title = "Building REST APIs with Minimal APIs",
                Content = "<h2>Minimal APIs in .NET 10</h2>" +
                    "<p>Minimal APIs offer a lightweight alternative to controllers for building HTTP APIs.</p>" +
                    "<h3>Basic Endpoint</h3>" +
                    "<pre><code>app.MapGet(\"/api/documents\", async (IMediator mediator) =>\n{\n    var docs = await mediator.Send(new GetAllDocumentsQuery());\n    return Results.Ok(docs);\n});</code></pre>" +
                    "<h3>TODO</h3>" +
                    "<ul>" +
                    "<li>Add validation examples</li>" +
                    "<li>Add error handling middleware</li>" +
                    "<li>Compare with controller-based approach</li>" +
                    "</ul>",
                AuthorId = member.Id,
                CategoryId = catBackend.Id,
                Status = ContentStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Tags = [tagRestApi, tagAspnet, tagDotnet, tagMediatr]
            };

            context.BlogPosts.AddRange(blog1, blog2, blog3, blog4, blog5, blog6);
            await context.SaveChangesAsync();
            result.BlogPostsCreated = 6;

            // ────────────────────────────────
            //  ANNOUNCEMENTS  (3 total)
            // ────────────────────────────────
            context.Announcements.AddRange(
                new Announcement
                {
                    Title = "Welcome to the Corporate Knowledge Base",
                    Content = "Welcome to our internal knowledge sharing platform! Here you can find technical documents, blog posts, FAQs, and team announcements. Every team member is encouraged to contribute — start by creating a document or blog post from the navigation menu.",
                    AuthorId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new Announcement
                {
                    Title = "New: Content Review Workflow",
                    Content = "We've introduced a content review process to ensure quality across the knowledge base. When you create a document or blog post, it starts as a Draft. Submit it for review, and an Editor or Admin will approve it for publication. Editors can now access the Content Review panel from the navigation menu.",
                    AuthorId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Announcement
                {
                    Title = "Department-Based Access Control Now Available",
                    Content = "You can now restrict documents and blog posts to specific departments. When creating or editing content, select a department from the dropdown to limit visibility. Content without a department restriction remains visible to all authenticated users. Editors and Admins can always access all content regardless of department.",
                    AuthorId = admin.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            );
            await context.SaveChangesAsync();
            result.AnnouncementsCreated = 3;

            // ────────────────────────────────
            //  COMMENTS
            // ────────────────────────────────
            var allDocs = await context.Documents.OrderBy(d => d.CreatedAt).ToListAsync();
            var allPosts = await context.BlogPosts.OrderBy(b => b.CreatedAt).ToListAsync();

            context.Comments.AddRange(
                new Comment { Content = "Excellent overview! This really helped me understand the layer boundaries. One question — where would you place background job services like Hangfire?", AuthorId = member.Id, DocumentId = allDocs[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-43) },
                new Comment { Content = "Background services would go in the Infrastructure layer since they interact with external concerns. The interface/abstraction goes in Application.", AuthorId = editor.Id, DocumentId = allDocs[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-42) },
                new Comment { Content = "I'd also recommend adding a section on dependency injection configuration — it's the glue that makes this architecture work.", AuthorId = devopsUser.Id, DocumentId = allDocs[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-40) },
                new Comment { Content = "Great article! We've been using this pattern for 6 months now and it's transformed how we organize features.", AuthorId = member.Id, DocumentId = allDocs[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-35) },
                new Comment { Content = "How do you handle cross-cutting concerns like logging and validation? Pipeline behaviors?", AuthorId = qaUser.Id, DocumentId = allDocs[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-34) },
                new Comment { Content = "The AsNoTracking tip alone saved us 200ms on our dashboard query. Highly recommend applying this everywhere you can!", AuthorId = devopsUser.Id, DocumentId = allDocs[3].Id, CreatedAt = DateTime.UtcNow.AddDays(-23) },
                new Comment { Content = "Can't wait to upgrade our projects to .NET 10! The EF Core improvements look particularly promising.", AuthorId = member.Id, BlogPostId = allPosts[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-38) },
                new Comment { Content = "We already migrated two services to .NET 10 — the performance gains are real, especially for JSON-heavy APIs.", AuthorId = editor.Id, BlogPostId = allPosts[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-36) },
                new Comment { Content = "This mirrors our experience exactly. The reduced merge conflicts alone made the switch worthwhile.", AuthorId = admin.Id, BlogPostId = allPosts[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-28) },
                new Comment { Content = "Have you considered adding automated security scanning (SAST) to the pipeline? Tools like Snyk integrate well with GitHub Actions.", AuthorId = qaUser.Id, BlogPostId = allPosts[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Comment { Content = "Nice comparison with Bootstrap. For admin panels, I still prefer Bootstrap — but for customer-facing UI, Tailwind is the way to go.", AuthorId = editor.Id, BlogPostId = allPosts[3].Id, CreatedAt = DateTime.UtcNow.AddDays(-12) }
            );
            await context.SaveChangesAsync();
            result.CommentsCreated = 11;
        }

        // ═══════════════════════════════════════════
        //  FAQ ITEMS
        // ═══════════════════════════════════════════
        if (!await context.FaqItems.AnyAsync())
        {
            context.FaqItems.AddRange(
                new FaqItem { Question = "How do I get access to the knowledge base?", Answer = "Register an account using the <strong>Register</strong> page. Your account will be in <em>Pending</em> status until an administrator approves it. Once approved, you can log in and start contributing content.", Category = "General", SortOrder = 1, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "What are the user roles and permissions?", Answer = "<ul><li><strong>Member</strong> — Can create, edit, and delete their own content. New content starts as Draft.</li><li><strong>Editor</strong> — Can edit anyone's content and approve/reject pending reviews.</li><li><strong>Admin</strong> — Full access including user management, department management, and all content operations.</li></ul>", Category = "General", SortOrder = 2, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "How does the content review process work?", Answer = "Content goes through the following lifecycle:<br/><ol><li><strong>Draft</strong> — Only visible to the author</li><li><strong>Pending Review</strong> — Submitted for review, visible to Editors/Admins</li><li><strong>Published</strong> — Approved and visible to all users</li><li><strong>Rejected</strong> — Sent back to the author with feedback</li></ol>Admins can publish content directly without review.", Category = "General", SortOrder = 3, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "What technology stack does this platform use?", Answer = "<strong>Backend:</strong> .NET 10 LTS, ASP.NET Core MVC, Entity Framework Core 10, MediatR (CQRS), SQL Server<br/><strong>Frontend:</strong> Bootstrap 5, Tagify (tag management), Marked.js (Markdown rendering)<br/><strong>Architecture:</strong> Clean Architecture + Vertical Slice with feature folders", Category = "Development", SortOrder = 1, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "How do I link to other documents using wiki syntax?", Answer = "Use double brackets to create wiki-style links: <code>[[Document Title]]</code>. The system automatically resolves these to clickable links.<br/><br/>Examples:<br/><code>[[Clean Architecture Guide]]</code> → links to the matching document<br/><code>[[CI/CD Pipeline with GitHub Actions]]</code> → links to the matching blog post<br/><br/>If no matching content is found, the link appears in <span style='color:red'>red</span> to indicate a broken reference.", Category = "Development", SortOrder = 2, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "What is department-based access control?", Answer = "When creating content, you can optionally restrict it to a specific department. <strong>Restricted content</strong> is only visible to users in that department, plus Editors and Admins. <strong>Unrestricted content</strong> (no department selected) is visible to all authenticated users.<br/><br/>Example: A DevOps deployment guide might be restricted to the DevOps department, while a coding standards document would be left unrestricted for everyone.", Category = "Development", SortOrder = 3, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "How do I run the project locally for development?", Answer = "<ol><li>Clone the repository</li><li>Ensure <strong>.NET 10 SDK</strong> and <strong>SQL Server</strong> (or SQL Server Express/LocalDB) are installed</li><li>Update the connection string in <code>appsettings.Development.json</code></li><li>Run <code>dotnet ef database update</code> from the Infrastructure project</li><li>Run <code>dotnet run</code> from the Web project</li></ol><br/>Seed data automatically creates an admin account. Demo data can be loaded from the Admin panel.", Category = "DevOps", SortOrder = 1, IsPublished = true, CreatedAt = DateTime.UtcNow },
                new FaqItem { Question = "How do I deploy the application to production?", Answer = "The recommended approach is Docker-based deployment:<ol><li>Build the Docker image using the multi-stage Dockerfile</li><li>Push to your container registry (ACR, ECR, Docker Hub)</li><li>Deploy with Docker Compose or Kubernetes</li></ol>See the <strong>Docker Deployment Guide</strong> document and the <strong>CI/CD Pipeline</strong> blog post for detailed instructions.", Category = "DevOps", SortOrder = 2, IsPublished = true, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            result.FaqItemsCreated = 8;
        }

        result.Message = $"Demo verileri başarıyla yüklendi! " +
            $"{result.DepartmentsCreated} departman, {result.UsersCreated} kullanıcı, " +
            $"{result.CategoriesCreated} kategori, {result.TagsCreated} etiket, " +
            $"{result.DocumentsCreated} doküman, {result.BlogPostsCreated} blog yazısı, " +
            $"{result.AnnouncementsCreated} duyuru, {result.CommentsCreated} yorum, " +
            $"{result.FaqItemsCreated} SSS oluşturuldu.";

        return result;
    }

    // ╔═══════════════════════════════════════════════════════════════╗
    //  CLEAR DEMO DATA — Removes all demo content (keeps Admin)
    // ╚═══════════════════════════════════════════════════════════════╝
    public static async Task<string> ClearDemoDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        int removed = 0;

        // Comments first (FK dependency)
        var comments = await context.Comments.ToListAsync();
        context.Comments.RemoveRange(comments);
        removed += comments.Count;

        // Documents
        var docs = await context.Documents.ToListAsync();
        context.Documents.RemoveRange(docs);
        removed += docs.Count;

        // Blog Posts
        var blogs = await context.BlogPosts.ToListAsync();
        context.BlogPosts.RemoveRange(blogs);
        removed += blogs.Count;

        // Announcements
        var announcements = await context.Announcements.ToListAsync();
        context.Announcements.RemoveRange(announcements);
        removed += announcements.Count;

        // FAQ Items
        var faqs = await context.FaqItems.ToListAsync();
        context.FaqItems.RemoveRange(faqs);
        removed += faqs.Count;

        // Tags
        var tags = await context.Tags.ToListAsync();
        context.Tags.RemoveRange(tags);
        removed += tags.Count;

        // Categories
        var categories = await context.Categories.ToListAsync();
        context.Categories.RemoveRange(categories);
        removed += categories.Count;

        await context.SaveChangesAsync();

        // Remove demo users (keep admin@corp.com)
        string[] demoEmails = ["editor@corp.com", "member@corp.com", "devops@corp.com", "qa@corp.com", "pending@corp.com"];
        foreach (var email in demoEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await userManager.DeleteAsync(user);
                removed++;
            }
        }

        // Remove extra departments (keep Engineering)
        var extraDepts = await context.Departments.Where(d => d.Name != "Engineering").ToListAsync();
        context.Departments.RemoveRange(extraDepts);
        removed += extraDepts.Count;
        await context.SaveChangesAsync();

        return $"Demo verileri temizlendi. Toplam {removed} kayıt silindi. Admin hesabı ve Engineering departmanı korundu.";
    }

    /// <summary>
    /// Creates a user if not exists, ensures correct role and approval state.
    /// </summary>
    private static async Task<ApplicationUser?> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser userData,
        string password,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(userData.Email!);
        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, role))
                await userManager.AddToRoleAsync(existing, role);

            if (existing.IsApproved != userData.IsApproved)
            {
                existing.IsApproved = userData.IsApproved;
                await userManager.UpdateAsync(existing);
            }
            return existing;
        }

        var result = await userManager.CreateAsync(userData, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(userData, role);
            return userData;
        }
        return null;
    }
}

/// <summary>
/// Result of demo data seeding operation
/// </summary>
public class SeedResult
{
    public bool AlreadySeeded { get; set; }
    public string Message { get; set; } = "";
    public int DepartmentsCreated { get; set; }
    public int UsersCreated { get; set; }
    public int CategoriesCreated { get; set; }
    public int TagsCreated { get; set; }
    public int DocumentsCreated { get; set; }
    public int BlogPostsCreated { get; set; }
    public int AnnouncementsCreated { get; set; }
    public int CommentsCreated { get; set; }
    public int FaqItemsCreated { get; set; }
}
