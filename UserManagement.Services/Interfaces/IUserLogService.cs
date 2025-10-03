using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IUserLogService
{
    Task LogAsync(long userId, UserActionType action, string? details = null, string? performedBy = null, CancellationToken ct = default);

    Task<IEnumerable<UserLog>> GetForUserAsync(long userId, int take = 10, CancellationToken ct = default);

    Task<(IEnumerable<UserLog> Items, int Total)> GetAllAsync(int page = 1, int pageSize = 25, string? query = null, CancellationToken ct = default);

    Task<UserLog?> GetByIdAsync(long id, CancellationToken ct = default);
}
