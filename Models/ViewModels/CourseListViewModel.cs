using GiuaKyWeb.Models;

namespace GiuaKyWeb.Models.ViewModels;

public class CourseListViewModel
{
    public IReadOnlyList<Course> Courses { get; set; } = Array.Empty<Course>();

    public string? SearchTerm { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }
}
