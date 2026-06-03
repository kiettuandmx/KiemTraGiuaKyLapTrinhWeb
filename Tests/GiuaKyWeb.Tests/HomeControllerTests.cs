using GiuaKyWeb.Controllers;
using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiuaKyWeb.Tests;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ReturnsFiveCoursesForRequestedPage()
    {
        await using var context = BuildContext(nameof(Index_ReturnsFiveCoursesForRequestedPage));
        context.Categories.Add(new Category { Id = 1, Name = "Software" });
        context.Courses.AddRange(Enumerable.Range(1, 12).Select(i => new Course
        {
            Id = i,
            Name = $"Course {i:00}",
            CategoryId = 1,
            Credits = 3,
            Lecturer = "Lecturer"
        }));
        await context.SaveChangesAsync();
        Assert.Equal(12, await context.Courses.CountAsync());

        var controller = new HomeController(context);

        var result = await controller.Index(null, 2) as ViewResult;
        var model = Assert.IsType<CourseListViewModel>(result!.Model);

        Assert.Equal(5, model.Courses.Count);
        Assert.Equal("Course 06", model.Courses.First().Name);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(3, model.TotalPages);
    }

    [Fact]
    public async Task Index_FiltersCoursesBySearchTerm()
    {
        await using var context = BuildContext(nameof(Index_FiltersCoursesBySearchTerm));
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Software" },
            new Category { Id = 2, Name = "Database" });
        context.Courses.AddRange(
            new Course { Name = "Web Programming", CategoryId = 1, Credits = 3, Lecturer = "Lecturer A" },
            new Course { Name = "Database Systems", CategoryId = 2, Credits = 3, Lecturer = "Lecturer B" });
        await context.SaveChangesAsync();
        Assert.Equal(2, await context.Courses.CountAsync());

        var controller = new HomeController(context);

        var result = await controller.Index("Web", 1) as ViewResult;
        var model = Assert.IsType<CourseListViewModel>(result!.Model);

        Assert.Single(model.Courses);
        Assert.Equal("Web Programming", model.Courses[0].Name);
        Assert.Equal("Web", model.SearchTerm);
    }

    private static ApplicationDbContext BuildContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
