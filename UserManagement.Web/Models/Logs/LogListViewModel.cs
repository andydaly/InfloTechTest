namespace UserManagement.Web.Models.Logs;

public class LogListViewModel
{
    public List<LogListItemViewModel> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int Total { get; set; }
    public string? Query { get; set; }

    public int TotalPages => PageSize == 0 ? 1 : (int)System.Math.Ceiling((double)Total / PageSize);
}
