using System.ComponentModel.DataAnnotations;

namespace Web_Project.Models;

public class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = "";

    [Required]
    public string City { get; set; } = "";

    [Phone]
    [Display(Name = "Phone number")]
    public string? Phone { get; set; }

    [Required]
    [RegularExpression("^(Donor|Requester|Hospital)$", ErrorMessage = "Choose a valid account type.")]
    public string Role { get; set; } = "";

    [DataType(DataType.Password), Required, MinLength(8)]
    public string Password { get; set; } = "";

    [DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";

    [Display(Name = "Blood group")]
    public string? BloodGroup { get; set; }
}
