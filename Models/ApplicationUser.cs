using Microsoft.AspNetCore.Identity;

namespace Web_Project.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public string City { get; set; } = "";
    public string AccountType { get; set; } = "Requester";
    public string? BloodGroup { get; set; }
}

public class BloodRequest
{
    public int Id { get; set; }
    public string BloodGroup { get; set; } = "";
    public int Units { get; set; }
    public string Hospital { get; set; } = "";
    public string RequesterId { get; set; } = "";
    public ApplicationUser? Requester { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Matching";
}
