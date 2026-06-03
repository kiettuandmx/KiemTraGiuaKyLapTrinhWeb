using GiuaKyWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiuaKyWeb.Tests;

public class RoutingTests
{
    [Fact]
    public void HomeIndex_HasPublicRoutesForHomeAndCourses()
    {
        var method = typeof(HomeController).GetMethod(nameof(HomeController.Index))!;
        var routes = method.GetCustomAttributes(typeof(HttpGetAttribute), false)
            .Cast<HttpGetAttribute>()
            .Select(x => x.Template)
            .ToArray();

        Assert.Contains(string.Empty, routes);
        Assert.Contains("home", routes);
        Assert.Contains("courses", routes);
    }

    [Fact]
    public void CoursesController_UsesAdminRoutePrefix()
    {
        var route = typeof(CoursesController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .SingleOrDefault();

        Assert.NotNull(route);
        Assert.Equal("admin/courses", route!.Template);
    }

    [Fact]
    public void EnrollmentsController_UsesEnrollRoutePrefix()
    {
        var route = typeof(EnrollmentsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .SingleOrDefault();

        Assert.NotNull(route);
        Assert.Equal("enroll", route!.Template);
    }
}
