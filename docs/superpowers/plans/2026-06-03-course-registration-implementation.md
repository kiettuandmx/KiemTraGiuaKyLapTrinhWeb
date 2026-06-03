# Course Registration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the MVC course registration application described in `exam.md` using ASP.NET Core MVC, EF Core, ASP.NET Core Identity, and MySQL in Laragon.

**Architecture:** Extend the current starter MVC app into a server-rendered application with Identity-backed authentication, EF Core persistence, role-based authorization, and Razor views for public, admin, and student workflows. Keep business logic close to the MVC app structure: `Data` for persistence, `Models` for entities and view models, `Controllers` for workflows, and `Views` for UI.

**Tech Stack:** ASP.NET Core MVC, EF Core, ASP.NET Core Identity, Pomelo MySQL provider, Bootstrap, xUnit test project

---

## File Structure

### Create

- `Data/ApplicationDbContext.cs`
- `Data/DbInitializer.cs`
- `Models/ApplicationUser.cs`
- `Models/Category.cs`
- `Models/Course.cs`
- `Models/Enrollment.cs`
- `Models/ViewModels/CourseListViewModel.cs`
- `Models/ViewModels/CourseFormViewModel.cs`
- `Models/ViewModels/MyCoursesViewModel.cs`
- `Models/ViewModels/RegisterViewModel.cs`
- `Models/ViewModels/LoginViewModel.cs`
- `Controllers/CoursesController.cs`
- `Controllers/EnrollmentsController.cs`
- `Controllers/AccountController.cs`
- `Views/Courses/Index.cshtml`
- `Views/Courses/Create.cshtml`
- `Views/Courses/Edit.cshtml`
- `Views/Courses/Delete.cshtml`
- `Views/Enrollments/MyCourses.cshtml`
- `Views/Account/Register.cshtml`
- `Views/Account/Login.cshtml`
- `Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj`
- `Tests/GiuaKyWeb.Tests/HomeControllerTests.cs`
- `Tests/GiuaKyWeb.Tests/EnrollmentRulesTests.cs`
- `Tests/GiuaKyWeb.Tests/AccountControllerTests.cs`

### Modify

- `GiuaKyWeb.csproj`
- `Program.cs`
- `appsettings.json`
- `appsettings.Development.json`
- `Controllers/HomeController.cs`
- `Views/Home/Index.cshtml`
- `Views/Shared/_Layout.cshtml`
- `Views/_ViewImports.cshtml`

### Deferred by generated tooling during implementation

- `Migrations/*`

---

### Task 1: Add persistence and auth dependencies

**Files:**
- Modify: `GiuaKyWeb.csproj`

- [ ] **Step 1: Write the failing test**

Create `Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj` with a first compile-only test project reference that expects the web project to expose EF Core and MVC types used by later tests.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\..\\GiuaKyWeb.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj`

Expected: FAIL because the test project compiles against a web project that does not yet include the Identity and EF Core packages required by later tests.

- [ ] **Step 3: Write minimal implementation**

Update `GiuaKyWeb.csproj` to include the required packages.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0-preview.7.25380.108" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-preview.7.25380.108">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="10.0.0-preview.2.efcore.10.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="10.0.0-preview.7.25380.108" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj`

Expected: PASS for restore/build of the baseline test project.

- [ ] **Step 5: Commit**

```bash
git add GiuaKyWeb.csproj Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj
git commit -m "build: add ef core identity and test dependencies"
```

### Task 2: Add domain models and DbContext

**Files:**
- Create: `Models/ApplicationUser.cs`
- Create: `Models/Category.cs`
- Create: `Models/Course.cs`
- Create: `Models/Enrollment.cs`
- Create: `Data/ApplicationDbContext.cs`
- Test: `Tests/GiuaKyWeb.Tests/EnrollmentRulesTests.cs`

- [ ] **Step 1: Write the failing test**

Create a test proving duplicate enrollments should violate a unique user-course combination.

```csharp
using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace GiuaKyWeb.Tests;

public class EnrollmentRulesTests
{
    [Fact]
    public void OnModelCreating_ConfiguresUniqueEnrollmentIndex()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("model-check")
            .Options;

        using var context = new ApplicationDbContext(options);

        var entity = context.Model.FindEntityType(typeof(Enrollment));
        var index = entity!.GetIndexes().SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { "UserId", "CourseId" }));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter OnModelCreating_ConfiguresUniqueEnrollmentIndex`

Expected: FAIL because `ApplicationDbContext` and `Enrollment` do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Create the entity and context files.

```csharp
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace GiuaKyWeb.Models;

public class ApplicationUser : IdentityUser
{
}
```

```csharp
// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

```csharp
// Models/Course.cs
using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Image URL")]
    public string? Image { get; set; }

    [Range(1, 10)]
    public int Credits { get; set; }

    [Required, StringLength(150)]
    public string Lecturer { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

```csharp
// Models/Enrollment.cs
namespace GiuaKyWeb.Models;

public class Enrollment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public ApplicationUser? User { get; set; }
    public Course? Course { get; set; }
}
```

```csharp
// Data/ApplicationDbContext.cs
using GiuaKyWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiuaKyWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter OnModelCreating_ConfiguresUniqueEnrollmentIndex`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Data/ApplicationDbContext.cs Models/ApplicationUser.cs Models/Category.cs Models/Course.cs Models/Enrollment.cs Tests/GiuaKyWeb.Tests/EnrollmentRulesTests.cs
git commit -m "feat: add course registration domain model"
```

### Task 3: Configure MySQL, Identity, roles, and seed logic

**Files:**
- Create: `Data/DbInitializer.cs`
- Modify: `Program.cs`
- Modify: `appsettings.json`
- Modify: `appsettings.Development.json`
- Test: `Tests/GiuaKyWeb.Tests/AccountControllerTests.cs`

- [ ] **Step 1: Write the failing test**

Create a test that proves new registration should assign the `Student` role through account workflow dependencies.

```csharp
using GiuaKyWeb.Controllers;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiuaKyWeb.Tests;

public class AccountControllerTests
{
    [Fact]
    public async Task Register_Post_AssignsStudentRole()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = MockUserManager(userStore.Object);
        var signInManager = MockSignInManager(userManager.Object);

        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();

        var controller = new AccountController(userManager.Object, signInManager.Object);
        var model = new RegisterViewModel
        {
            UserName = "student1",
            Email = "student1@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await controller.Register(model);

        Assert.IsType<RedirectToActionResult>(result);
        userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"), Times.Once);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Register_Post_AssignsStudentRole`

Expected: FAIL because `AccountController`, view models, and auth setup do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add configuration and seed logic.

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=giuakyweb_db;user=root;password=;"
  },
  "Authentication": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  },
  "SeedAdmin": {
    "Email": "admin@local.test",
    "Password": "Admin123!",
    "UserName": "admin"
  }
}
```

```csharp
// Data/DbInitializer.cs
using GiuaKyWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace GiuaKyWeb.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        foreach (var role in new[] { "Admin", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        var userName = configuration["SeedAdmin:UserName"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
```

```csharp
// Program.cs
using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection was not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

app.Run();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Register_Post_AssignsStudentRole`

Expected: PASS once the account workflow exists in the next task and role wiring is available.

- [ ] **Step 5: Commit**

```bash
git add Program.cs appsettings.json appsettings.Development.json Data/DbInitializer.cs
git commit -m "feat: configure mysql identity and role seeding"
```

### Task 4: Add account workflow for register, login, logout, and Google sign-in

**Files:**
- Create: `Models/ViewModels/RegisterViewModel.cs`
- Create: `Models/ViewModels/LoginViewModel.cs`
- Create: `Controllers/AccountController.cs`
- Create: `Views/Account/Register.cshtml`
- Create: `Views/Account/Login.cshtml`
- Test: `Tests/GiuaKyWeb.Tests/AccountControllerTests.cs`

- [ ] **Step 1: Write the failing test**

Expand the earlier test file with a second test for successful login redirect.

```csharp
[Fact]
public async Task Login_Post_RedirectsToHome_WhenCredentialsAreValid()
{
    var userStore = new Mock<IUserStore<ApplicationUser>>();
    var userManager = MockUserManager(userStore.Object);
    var signInManager = MockSignInManager(userManager.Object);

    signInManager.Setup(x => x.PasswordSignInAsync("student1", "Password123!", false, false))
        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

    var controller = new AccountController(userManager.Object, signInManager.Object);

    var result = await controller.Login(new LoginViewModel
    {
        UserName = "student1",
        Password = "Password123!"
    });

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
    Assert.Equal("Home", redirect.ControllerName);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter "Register_Post_AssignsStudentRole|Login_Post_RedirectsToHome_WhenCredentialsAreValid"`

Expected: FAIL because account controller and models are still missing.

- [ ] **Step 3: Write minimal implementation**

Create view models and controller actions.

```csharp
// Models/ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

```csharp
// Models/ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

```csharp
// Controllers/AccountController.cs
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiuaKyWeb.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Student");
        await _signInManager.SignInAsync(user, false);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (signInResult.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction(nameof(Login));
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter "Register_Post_AssignsStudentRole|Login_Post_RedirectsToHome_WhenCredentialsAreValid"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Controllers/AccountController.cs Models/ViewModels/RegisterViewModel.cs Models/ViewModels/LoginViewModel.cs Views/Account/Register.cshtml Views/Account/Login.cshtml Tests/GiuaKyWeb.Tests/AccountControllerTests.cs
git commit -m "feat: add account registration and login flow"
```

### Task 5: Build home page course list, search, and pagination

**Files:**
- Create: `Models/ViewModels/CourseListViewModel.cs`
- Modify: `Controllers/HomeController.cs`
- Modify: `Views/Home/Index.cshtml`
- Test: `Tests/GiuaKyWeb.Tests/HomeControllerTests.cs`

- [ ] **Step 1: Write the failing test**

Create a test for page size `5` and search filtering.

```csharp
using GiuaKyWeb.Controllers;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GiuaKyWeb.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsFiveCoursesForRequestedPage()
    {
        var courses = Enumerable.Range(1, 12)
            .Select(i => new Course { Id = i, Name = $"Course {i}", Credits = 3, Lecturer = "Lecturer" })
            .ToList();

        var controller = new HomeController();
        controller.LoadCoursesForTest(courses);

        var result = controller.Index(null, 2) as ViewResult;
        var model = Assert.IsType<CourseListViewModel>(result!.Model);

        Assert.Equal(5, model.Courses.Count);
        Assert.Equal("Course 6", model.Courses.First().Name);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Index_ReturnsFiveCoursesForRequestedPage`

Expected: FAIL because the home controller does not support injected course data, search, or pagination model.

- [ ] **Step 3: Write minimal implementation**

Implement a paged list view model and home query logic, but use dependency injection instead of a test-only method when coding for real. The production shape should look like this:

```csharp
// Models/ViewModels/CourseListViewModel.cs
using GiuaKyWeb.Models;

namespace GiuaKyWeb.Models.ViewModels;

public class CourseListViewModel
{
    public IReadOnlyList<Course> Courses { get; set; } = Array.Empty<Course>();
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
```

```csharp
// Controllers/HomeController.cs
using GiuaKyWeb.Data;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GiuaKyWeb.Controllers;

public class HomeController : Controller
{
    private const int PageSize = 5;
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm));
        }

        var totalCourses = await query.CountAsync();
        var courses = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var model = new CourseListViewModel
        {
            Courses = courses,
            SearchTerm = searchTerm,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCourses / (double)PageSize)
        };

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Index_ReturnsFiveCoursesForRequestedPage`

Expected: PASS after replacing the test-only setup with a real controller test using an in-memory context.

- [ ] **Step 5: Commit**

```bash
git add Controllers/HomeController.cs Models/ViewModels/CourseListViewModel.cs Views/Home/Index.cshtml Tests/GiuaKyWeb.Tests/HomeControllerTests.cs
git commit -m "feat: add course list search and pagination"
```

### Task 6: Add admin course CRUD

**Files:**
- Create: `Models/ViewModels/CourseFormViewModel.cs`
- Create: `Controllers/CoursesController.cs`
- Create: `Views/Courses/Index.cshtml`
- Create: `Views/Courses/Create.cshtml`
- Create: `Views/Courses/Edit.cshtml`
- Create: `Views/Courses/Delete.cshtml`

- [ ] **Step 1: Write the failing test**

Add a test that proves `Create` redirects back to admin list when the model is valid.

```csharp
[Fact]
public async Task Create_Post_PersistsCourseAndRedirects()
{
    var context = BuildContext();
    context.Categories.Add(new Category { Id = 1, Name = "Software" });
    await context.SaveChangesAsync();

    var controller = new CoursesController(context);

    var result = await controller.Create(new CourseFormViewModel
    {
        Name = "ASP.NET Core",
        Credits = 3,
        Lecturer = "Mr A",
        CategoryId = 1,
        Image = "https://example.com/course.jpg"
    });

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal(nameof(CoursesController.Index), redirect.ActionName);
    Assert.Single(context.Courses);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Create_Post_PersistsCourseAndRedirects`

Expected: FAIL because the controller and form model do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Create a focused admin controller with role protection and standard MVC actions.

```csharp
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.Include(c => c.Category).ToListAsync();
        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }

        _context.Courses.Add(new Course
        {
            Name = model.Name,
            Credits = model.Credits,
            Lecturer = model.Lecturer,
            CategoryId = model.CategoryId,
            Image = model.Image
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Create_Post_PersistsCourseAndRedirects`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Controllers/CoursesController.cs Models/ViewModels/CourseFormViewModel.cs Views/Courses
git commit -m "feat: add admin course management"
```

### Task 7: Add student enrollment and My Courses

**Files:**
- Create: `Models/ViewModels/MyCoursesViewModel.cs`
- Create: `Controllers/EnrollmentsController.cs`
- Create: `Views/Enrollments/MyCourses.cshtml`
- Test: `Tests/GiuaKyWeb.Tests/EnrollmentRulesTests.cs`

- [ ] **Step 1: Write the failing test**

Add a controller-level test for duplicate enrollment rejection.

```csharp
[Fact]
public async Task Enroll_Post_DoesNotCreateDuplicateEnrollment()
{
    var context = BuildContext();
    context.Courses.Add(new Course { Id = 1, Name = "Math", Credits = 3, Lecturer = "L1" });
    context.Enrollments.Add(new Enrollment { UserId = "user-1", CourseId = 1, EnrollDate = DateTime.UtcNow });
    await context.SaveChangesAsync();

    var controller = BuildEnrollmentController(context, "user-1");

    await controller.Enroll(1);

    Assert.Single(context.Enrollments);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Enroll_Post_DoesNotCreateDuplicateEnrollment`

Expected: FAIL because the enrollment controller does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add a student-only controller that derives the current user id from claims and blocks duplicates.

```csharp
[Authorize(Roles = "Student")]
public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var course = await _context.Courses.FindAsync(courseId);
        if (course is null)
        {
            return NotFound();
        }

        var exists = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (!exists)
        {
            _context.Enrollments.Add(new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);
        var enrollment = await _context.Enrollments.SingleOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (enrollment is null)
        {
            return NotFound();
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyCourses));
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj --filter Enroll_Post_DoesNotCreateDuplicateEnrollment`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Controllers/EnrollmentsController.cs Models/ViewModels/MyCoursesViewModel.cs Views/Enrollments/MyCourses.cshtml Tests/GiuaKyWeb.Tests/EnrollmentRulesTests.cs
git commit -m "feat: add student enrollment workflow"
```

### Task 8: Finish shared layout, authorization-aware navigation, and responsive views

**Files:**
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Home/Index.cshtml`
- Modify: `Views/Account/Register.cshtml`
- Modify: `Views/Account/Login.cshtml`
- Modify: `Views/Courses/*.cshtml`
- Modify: `Views/Enrollments/MyCourses.cshtml`
- Modify: `Views/_ViewImports.cshtml`

- [ ] **Step 1: Write the failing test**

Write a manual verification checklist file in the test project comments because this repo has no browser UI harness yet.

```csharp
/*
Manual UI verification checklist:
1. Navbar shows Register/Login when signed out.
2. Navbar shows My Courses for Student.
3. Navbar shows Manage Courses for Admin.
4. Home page search preserves value after submit.
5. Pagination renders correctly on mobile width.
*/
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet build`

Expected: BUILD succeeds, but the feature is still considered failing because the UI and layout behavior are not yet implemented and the manual checklist cannot pass.

- [ ] **Step 3: Write minimal implementation**

Apply these UI rules:

```cshtml
@* _Layout.cshtml navigation shape *@
<ul class="navbar-nav ms-auto">
    @if (User.Identity?.IsAuthenticated == true)
    {
        @if (User.IsInRole("Student"))
        {
            <li class="nav-item"><a class="nav-link" asp-controller="Enrollments" asp-action="MyCourses">My Courses</a></li>
        }
        @if (User.IsInRole("Admin"))
        {
            <li class="nav-item"><a class="nav-link" asp-controller="Courses" asp-action="Index">Manage Courses</a></li>
        }
        <li class="nav-item">
            <form asp-controller="Account" asp-action="Logout" method="post">
                <button class="btn btn-link nav-link" type="submit">Logout</button>
            </form>
        </li>
    }
    else
    {
        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Register">Register</a></li>
        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Login">Login</a></li>
    }
</ul>
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet build`

Expected: BUILD succeeds and the manual checklist can be validated by running the app.

- [ ] **Step 5: Commit**

```bash
git add Views/Shared/_Layout.cshtml Views/Home/Index.cshtml Views/Account Views/Courses Views/Enrollments Views/_ViewImports.cshtml
git commit -m "feat: add responsive mvc views and role-aware navigation"
```

### Task 9: Create migrations, update database, and verify end-to-end behavior

**Files:**
- Create: `Migrations/*`

- [ ] **Step 1: Write the failing test**

Define the final verification matrix before schema creation:

```text
1. App starts with MySQL connection to giuakyweb_db.
2. Roles Admin and Student exist.
3. Admin can create/edit/delete course.
4. Student can register/login/enroll/cancel.
5. Search and pagination work on home page.
6. Google login button appears only when configured.
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet ef migrations add InitialCreate`

Expected: FAIL until the project compiles cleanly with all data and auth pieces in place.

- [ ] **Step 3: Write minimal implementation**

Generate and apply the schema.

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet ef` is not available globally, use:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
dotnet test Tests/GiuaKyWeb.Tests/GiuaKyWeb.Tests.csproj
dotnet build
dotnet run
```

Expected:

- all tests PASS
- build succeeds with no compile errors
- app starts and serves the MVC site

- [ ] **Step 5: Commit**

```bash
git add Migrations
git commit -m "feat: finalize database schema and verify course registration app"
```

## Self-Review

- Spec coverage: the plan covers public listing, search, pagination, admin CRUD, registration, login, authorization, enrollment, My Courses, Google login, responsive UI, and MySQL setup.
- Placeholder scan: each task names exact files and concrete commands; no `TODO` or `TBD` markers remain.
- Type consistency: all tasks use the same `ApplicationDbContext`, `ApplicationUser`, `Course`, `Enrollment`, `RegisterViewModel`, `LoginViewModel`, and role names `Admin`/`Student`.
