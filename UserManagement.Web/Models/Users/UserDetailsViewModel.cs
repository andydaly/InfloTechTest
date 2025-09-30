using UserManagement.Models;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    public User User { get; set; } = default!;
    public List<UserLog> RecentLogs { get; set; } = new();
}
