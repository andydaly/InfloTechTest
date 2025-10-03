namespace UserManagement.BlazorWeb.Dtos;

public class UserLogDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Action { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public string? PerformedBy { get; set; }
    public string? Details { get; set; }
}
