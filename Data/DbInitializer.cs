using GiuaKyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiuaKyWeb.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
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

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

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

        await SeedCatalogDataAsync(context);
    }

    public static async Task SeedCatalogDataAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            if (!await context.Courses.AnyAsync())
            {
                await SeedDefaultCoursesAsync(context);
            }

            return;
        }

        var categories = new[]
        {
            new Category { Name = "Software Engineering" },
            new Category { Name = "Web Development" },
            new Category { Name = "Database Systems" },
            new Category { Name = "Information Security" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        await SeedDefaultCoursesAsync(context);
    }

    private static async Task SeedDefaultCoursesAsync(ApplicationDbContext context)
    {
        if (await context.Courses.AnyAsync())
        {
            return;
        }

        var categories = await context.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        var courses = new[]
        {
            new Course
            {
                Name = "Lập trình C#",
                Credits = 3,
                Lecturer = "Nguyen Van An",
                CategoryId = categories["Software Engineering"],
                Image = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Lập trình Java",
                Credits = 3,
                Lecturer = "Tran Thi Bich",
                CategoryId = categories["Software Engineering"],
                Image = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Lập trình Python",
                Credits = 3,
                Lecturer = "Le Quang Minh",
                CategoryId = categories["Software Engineering"],
                Image = "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Lập trình Web ASP.NET Core",
                Credits = 4,
                Lecturer = "Pham Thu Ha",
                CategoryId = categories["Web Development"],
                Image = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Cấu trúc dữ liệu và giải thuật",
                Credits = 4,
                Lecturer = "Vo Duc Long",
                CategoryId = categories["Software Engineering"],
                Image = "https://images.unsplash.com/photo-1504639725590-34d0984388bd?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Lập trình hướng đối tượng",
                Credits = 3,
                Lecturer = "Hoang Gia Bao",
                CategoryId = categories["Software Engineering"],
                Image = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Cơ sở dữ liệu",
                Credits = 3,
                Lecturer = "Dang Ngoc Lan",
                CategoryId = categories["Database Systems"],
                Image = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?auto=format&fit=crop&w=900&q=80"
            },
            new Course
            {
                Name = "Phát triển ứng dụng Frontend",
                Credits = 3,
                Lecturer = "Bui Thanh Son",
                CategoryId = categories["Web Development"],
                Image = "https://images.unsplash.com/photo-1498050108023-c5249f4df085?auto=format&fit=crop&w=900&q=80"
            }
        };

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
    }
}
