using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    public IEnumerable<User> FilterByActive(bool isActive)
        => _dataAccess.GetAll<User>().Where(u => u.IsActive == isActive).ToList();

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public User? GetById(long id)
        => _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);

    public void Create(User user)
    {
        _dataAccess.Create(user);
    }

    public bool Update(User user)
    {
        var existing = GetById(user.Id);
        if (existing is null) return false;

        existing.Forename = user.Forename;
        existing.Surname = user.Surname;
        existing.Email = user.Email;
        existing.IsActive = user.IsActive;
        existing.DateOfBirth = user.DateOfBirth;

        _dataAccess.Update(existing);
        return true;
    }

    public bool Delete(long id)
    {
        var existing = GetById(id);
        if (existing is null) return false;

        _dataAccess.Delete(existing);
        return true;
    }
}
