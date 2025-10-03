namespace UserManagement.BlazorWeb.Dtos;

public sealed class LogListResponse
{
    public IEnumerable<UserLogDto> Items { get; set; } = Enumerable.Empty<UserLogDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Query { get; set; }
}
