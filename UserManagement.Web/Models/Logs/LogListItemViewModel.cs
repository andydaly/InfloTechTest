using System;
using UserManagement.Data.Entities;

namespace UserManagement.Web.Models.Logs;

public class LogListItemViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public UserActionType Action { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? PerformedBy { get; set; }
    public string? Details { get; set; }
}
