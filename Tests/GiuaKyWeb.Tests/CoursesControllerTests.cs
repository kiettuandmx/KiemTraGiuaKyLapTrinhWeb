using GiuaKyWeb.Controllers;
using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiuaKyWeb.Tests;

public class CoursesControllerTests
{
    [Fact]
    public async Task Create_Post_PersistsCourseAndRedirects()
    {
        await using var context = BuildContext(nameof(Create_Post_PersistsCourseAndRedirects));
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

    private static ApplicationDbContext BuildContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
