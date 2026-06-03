using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models.ViewModels;

public class CourseFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Tên học phần")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Ảnh minh họa")]
    public string? Image { get; set; }

    [Range(1, 10)]
    [Display(Name = "Số tín chỉ")]
    public int Credits { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Giảng viên")]
    public string Lecturer { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }
}
