using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;

namespace GiuaKyWeb.Controllers;

public class HomeController : Controller
{
    private const int PageSize = 5;
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("home")]
    [HttpGet("courses")]
    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        page = Math.Max(page, 1);

        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm));
        }

        var totalCourses = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCourses / (double)PageSize));
        page = Math.Min(page, totalPages);

        var courses = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new CourseListViewModel
        {
            Courses = courses,
            SearchTerm = searchTerm,
            CurrentPage = page,
            TotalPages = totalPages
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
