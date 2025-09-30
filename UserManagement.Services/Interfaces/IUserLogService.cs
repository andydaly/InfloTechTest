using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;
public interface IUserLogService
{
    void Log(long userId, UserActionType action, string? details = null, string? performedBy = null);

    IEnumerable<UserLog> GetForUser(long userId, int take = 10);
    (IEnumerable<UserLog> Items, int Total) GetAll(int page = 1, int pageSize = 25, string? query = null);
    UserLog? GetById(long id);
}
