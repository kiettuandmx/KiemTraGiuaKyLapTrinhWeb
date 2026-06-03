using GiuaKyWeb.Data;
using GiuaKyWeb.Controllers;
using GiuaKyWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

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

    [Fact]
    public async Task Enroll_Post_DoesNotCreateDuplicateEnrollment()
    {
        await using var context = BuildContext(nameof(Enroll_Post_DoesNotCreateDuplicateEnrollment));
        context.Categories.Add(new Category { Id = 1, Name = "Software" });
        context.Courses.Add(new Course { Id = 1, Name = "Math", CategoryId = 1, Credits = 3, Lecturer = "L1" });
        context.Enrollments.Add(new Enrollment { UserId = "user-1", CourseId = 1, EnrollDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = BuildEnrollmentController(context, "user-1");

        await controller.Enroll(1);

        Assert.Single(context.Enrollments);
    }

    [Fact]
    public async Task Enroll_Post_SetsSuccessMessage_WhenEnrollmentCreated()
    {
        await using var context = BuildContext(nameof(Enroll_Post_SetsSuccessMessage_WhenEnrollmentCreated));
        context.Categories.Add(new Category { Id = 1, Name = "Software" });
        context.Courses.Add(new Course { Id = 1, Name = "Math", CategoryId = 1, Credits = 3, Lecturer = "L1" });
        await context.SaveChangesAsync();

        var controller = BuildEnrollmentController(context, "user-1");

        await controller.Enroll(1);

        Assert.Equal("Đăng ký học phần thành công.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Cancel_Post_SetsSuccessMessage_WhenEnrollmentRemoved()
    {
        await using var context = BuildContext(nameof(Cancel_Post_SetsSuccessMessage_WhenEnrollmentRemoved));
        context.Categories.Add(new Category { Id = 1, Name = "Software" });
        context.Courses.Add(new Course { Id = 1, Name = "Math", CategoryId = 1, Credits = 3, Lecturer = "L1" });
        context.Enrollments.Add(new Enrollment { Id = 10, UserId = "user-1", CourseId = 1, EnrollDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = BuildEnrollmentController(context, "user-1");

        await controller.Cancel(10);

        Assert.Equal("Hủy đăng ký học phần thành công.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task SeedCatalogDataAsync_AddsDefaultCategories_WhenEmpty()
    {
        await using var context = BuildContext(nameof(SeedCatalogDataAsync_AddsDefaultCategories_WhenEmpty));

        await DbInitializer.SeedCatalogDataAsync(context);

        Assert.NotEmpty(context.Categories);
        Assert.Contains(context.Categories, c => c.Name == "Software Engineering");
    }

    [Fact]
    public async Task SeedCatalogDataAsync_AddsProgrammingCourses_WhenCoursesEmpty()
    {
        await using var context = BuildContext(nameof(SeedCatalogDataAsync_AddsProgrammingCourses_WhenCoursesEmpty));

        await DbInitializer.SeedCatalogDataAsync(context);

        Assert.NotEmpty(context.Courses);
        Assert.Contains(context.Courses, c => c.Name == "Lập trình C#");
        Assert.Contains(context.Courses, c => c.Name == "Lập trình Web ASP.NET Core");
    }

    private static ApplicationDbContext BuildContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static EnrollmentsController BuildEnrollmentController(ApplicationDbContext context, string userId)
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        var controller = new EnrollmentsController(context, userManager.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity(
                            new[]
                            {
                                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Student")
                            },
                            "TestAuth"))
                }
            }
        };
        controller.TempData = new TempDataDictionary(controller.HttpContext, new TestTempDataProvider());

        return controller;
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object> _data = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return _data;
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            _data = new Dictionary<string, object>(values);
        }
    }
}
