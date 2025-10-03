using UserManagement.Data.Entities;

namespace UserManagement.API.Models;
public class LogListResponse
{
    public IEnumerable<UserLog> Items { get; set; } = Enumerable.Empty<UserLog>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Query { get; set; }
}
