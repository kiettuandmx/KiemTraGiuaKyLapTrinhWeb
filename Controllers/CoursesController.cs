using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiuaKyWeb.Controllers;

[Route("admin/courses")]
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(courses);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new CourseFormViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
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
            Image = model.Image,
            Credits = model.Credits,
            Lecturer = model.Lecturer,
            CategoryId = model.CategoryId
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync();

        return View(new CourseFormViewModel
        {
            Id = course.Id,
            Name = course.Name,
            Image = course.Image,
            Credits = course.Credits,
            Lecturer = course.Lecturer,
            CategoryId = course.CategoryId
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }

        var course = await _context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.Name = model.Name;
        course.Image = model.Image;
        course.Credits = model.Credits;
        course.Lecturer = model.Lecturer;
        course.CategoryId = model.CategoryId;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .SingleOrDefaultAsync(c => c.Id == id);

        return course is null ? NotFound() : View(course);
    }

    [HttpPost("delete/{id:int}")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CategoryId = new SelectList(categories, nameof(Category.Id), nameof(Category.Name));
    }
}
