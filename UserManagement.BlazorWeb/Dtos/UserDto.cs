using System.ComponentModel.DataAnnotations;

namespace UserManagement.BlazorWeb.Dtos;

public class UserDto
{
    public long Id { get; set; }
    [Required, StringLength(50)] public string Forename { get; set; } = "";
    [Required, StringLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }
}
