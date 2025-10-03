using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _data;
    public UserService(IDataContext data)
    {
        _data = data;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _data.GetAll<User>().AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<User>> FilterByActiveAsync(bool isActive, CancellationToken ct = default)
        => await _data.GetAll<User>().AsNoTracking().Where(u => u.IsActive == isActive).ToListAsync(ct);

    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
        => _data.FindAsync<User>(id).AsTask();

    public Task CreateAsync(User user, CancellationToken ct = default)
        => _data.CreateAsync(user);

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(user.Id, ct);
        if (existing is null) return false;

        existing.Forename = user.Forename;
        existing.Surname = user.Surname;
        existing.Email = user.Email;
        existing.IsActive = user.IsActive;
        existing.DateOfBirth = user.DateOfBirth;
        if (!string.IsNullOrWhiteSpace(user.Password))
            existing.Password = user.Password;
        await _data.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing is null) return false;

        await _data.DeleteAsync(existing);
        return true;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _data.GetAll<User>().AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
}
