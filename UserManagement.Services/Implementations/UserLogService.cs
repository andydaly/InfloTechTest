using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;
public class UserLogService : IUserLogService
{
    private readonly IDataContext _data;
    public UserLogService(IDataContext data) => _data = data;

    public void Log(long userId, UserActionType action, string? details = null, string? performedBy = null)
    {
        var entry = new UserLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            PerformedBy = performedBy
        };
        _data.Create(entry);
    }

    public IEnumerable<UserLog> GetForUser(long userId, int take = 10) => _data.GetAll<UserLog>().Where(l => l.UserId == userId).OrderByDescending(l => l.OccurredAt).Take(take).ToList();

    public (IEnumerable<UserLog> Items, int Total) GetAll(int page = 1, int pageSize = 25, string? query = null)
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

        var total = q.Count();
        var items = q.OrderByDescending(l => l.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return (items, total);
    }

    public UserLog? GetById(long id) => _data.GetAll<UserLog>().FirstOrDefault(l => l.Id == id);
}
