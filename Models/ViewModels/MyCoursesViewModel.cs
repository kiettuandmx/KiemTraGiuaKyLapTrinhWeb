using GiuaKyWeb.Models;

namespace GiuaKyWeb.Models.ViewModels;

public class MyCoursesViewModel
{
    public IReadOnlyList<Enrollment> Enrollments { get; set; } = Array.Empty<Enrollment>();
}
