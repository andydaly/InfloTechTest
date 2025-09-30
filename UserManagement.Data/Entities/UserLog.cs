using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;
public enum UserActionType
{
    Created,
    Viewed,
    Updated,
    Deleted
}

public class UserLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long UserId { get; set; }

    [Required]
    public UserActionType Action { get; set; }

    [Required]
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string? PerformedBy { get; set; } 

    [MaxLength(1000)]
    public string? Details { get; set; }
}
