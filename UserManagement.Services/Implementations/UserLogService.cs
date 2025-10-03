using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserLogService : IUserLogService
{
    private readonly IDataContext _data;
    public UserLogService(IDataContext data)
    {
        _data = data;
    }

    public async Task LogAsync(long userId, UserActionType action, string? details = null, string? performedBy = null, CancellationToken ct = default)
    {
        var entry = new UserLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            PerformedBy = performedBy
        };
        await _data.CreateAsync(entry);
    }

    public async Task<IEnumerable<UserLog>> GetForUserAsync(long userId, int take = 10, CancellationToken ct = default)
        => await _data.GetAll<UserLog>().Where(l => l.UserId == userId).OrderByDescending(l => l.OccurredAt).Take(take).ToListAsync(ct);

    public async Task<(IEnumerable<UserLog> Items, int Total)> GetAllAsync(int page = 1, int pageSize = 25, string? query = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 25;

        var q = _data.GetAll<UserLog>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.ToLowerInvariant();
            q = q.Where(l =>
                (l.Details ?? "").ToLower().Contains(term) ||
                (l.PerformedBy ?? "").ToLower().Contains(term) ||
                l.Action.ToString().ToLower().Contains(term) ||
                l.UserId.ToString().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(l => l.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<UserLog?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _data.GetAll<UserLog>().FirstOrDefaultAsync(l => l.Id == id, ct);
}
