using ERP.Application.Common.Interfaces;
using ERP.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User aggregate.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUserNameWithRolesAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }
}
