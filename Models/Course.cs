using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Image URL")]
    public string? Image { get; set; }

    [Range(1, 10)]
    public int Credits { get; set; }

    [Required]
    [StringLength(150)]
    public string Lecturer { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
