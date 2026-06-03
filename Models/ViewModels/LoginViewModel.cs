using System.ComponentModel.DataAnnotations;

namespace GiuaKyWeb.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;
}
