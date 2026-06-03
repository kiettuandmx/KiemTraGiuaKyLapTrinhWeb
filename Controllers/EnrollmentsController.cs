using GiuaKyWeb.Data;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiuaKyWeb.Controllers;

[Route("enroll")]
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

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
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
            TempData["SuccessMessage"] = "Đăng ký học phần thành công.";
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
            .ThenInclude(c => c!.Category)
            .OrderByDescending(e => e.EnrollDate)
            .ToListAsync();

        return View(new MyCoursesViewModel
        {
            Enrollments = enrollments
        });
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments.SingleOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (enrollment is null)
        {
            return NotFound();
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Hủy đăng ký học phần thành công.";
        return RedirectToAction(nameof(MyCourses));
    }
}
