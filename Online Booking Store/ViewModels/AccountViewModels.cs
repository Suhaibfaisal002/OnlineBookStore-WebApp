using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Online_Booking_Store.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(200)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }
}

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email or Username")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class AdminLoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class ProfileViewModel
{
    [Required, StringLength(200)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    public string? ProfileImagePath { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public int OrdersCount { get; set; }
}
