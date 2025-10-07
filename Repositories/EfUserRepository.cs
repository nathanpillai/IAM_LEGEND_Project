using IAMLegend.Data;
using IAMLegend.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAMLegend.Repositories
{
    public class EfUserRepository : IQueryUserRepository, ICommandUserRepository
    {
        private readonly ApplicationDbContext _db;
        public EfUserRepository(ApplicationDbContext db) { _db = db; }

        // Queries
        public Task<List<Domain>> GetDomainsAsync(CancellationToken ct = default) =>
            _db.Domains.OrderBy(d => d.domainname).ToListAsync(ct);

        public Task<List<Branch>> GetBranchesAsync(CancellationToken ct = default) =>
            _db.Branches.OrderBy(b => b.branchname).ToListAsync(ct);

        public Task<List<LocalSystem>> GetLocalSystemsAsync(CancellationToken ct = default) =>
            _db.LocalSystems.OrderBy(s => s.localsystemname).ToListAsync(ct);

        public Task<List<PermissionLevel>> GetPermissionLevelsBySystemAsync(int systemId, CancellationToken ct = default) =>
            _db.PermissionLevels.Where(p => p.localsystemid == systemId).OrderBy(p => p.name).ToListAsync(ct);

        public Task<List<UserProfile>> GetAllUsersAsync(CancellationToken ct = default) =>
            _db.UserProfiles.Where(u => u.status >= 0).OrderBy(u => u.firstname).ToListAsync(ct);

        public Task<UserProfile?> GetUserByIdAsync(int id, CancellationToken ct = default) =>
            _db.UserProfiles
               .Include(u => u.UserSystemAccesses)
               .Include(u => u.UserSystemBranches)
               .FirstOrDefaultAsync(u => u.userprofileid == id && u.status >= 0, ct);

        public Task<UserProfile?> GetUserByDomainUsernameAsync(string domain, string username, CancellationToken ct = default) =>
            _db.UserProfiles.FirstOrDefaultAsync(u => u.domain == domain && u.username == username && u.status >= 0, ct);

        public Task<List<UserSystemBranch>> GetUserSystemBranchesAsync(int userId, CancellationToken ct = default) =>
            _db.UserSystemBranches.Where(b => b.userprofileid == userId && b.status >= 0).ToListAsync(ct);

        public Task<List<UserSystemAccess>> GetUserSystemAccessAsync(int userId, CancellationToken ct = default) =>
            _db.UserSystemAccesses.Where(a => a.userprofileid == userId && a.status >= 0).ToListAsync(ct);

        // Commands
        public async Task<int> InsertUserProfileAsync(UserProfile user, CancellationToken ct = default)
        {
            _db.UserProfiles.Add(user);
            await _db.SaveChangesAsync(ct);
            return user.userprofileid;
        }

        public Task UpdateUserProfileAsync(UserProfile user, CancellationToken ct = default)
        {
            _db.UserProfiles.Update(user);
            return _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteUserProfileAsync(int userId, string modifiedBy, CancellationToken ct = default)
        {
            var user = await _db.UserProfiles.FindAsync(new object[] { userId }, ct);
            if (user == null) return;
            // Soft delete cascade handled by service (also soft deletes branches/access)
            user.status = -1;
            user.createddatetime = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public Task InsertUserSystemBranchAsync(UserSystemBranch branch, CancellationToken ct = default)
        {
            _db.UserSystemBranches.Add(branch);
            return _db.SaveChangesAsync(ct);
        }

        public Task InsertUserSystemAccessAsync(UserSystemAccess access, CancellationToken ct = default)
        {
            _db.UserSystemAccesses.Add(access);
            return _db.SaveChangesAsync(ct);
        }

        public Task SoftDeleteUserSystemBranchesAsync(int userId, string modifiedBy, CancellationToken ct = default)
        {
            var q = _db.UserSystemBranches.Where(b => b.userprofileid == userId && b.status >= 0);
            return q.ForEachAsync(b => b.status = -1, ct);
        }

        public Task SoftDeleteUserSystemAccessAsync(int userId, string modifiedBy, CancellationToken ct = default)
        {
            var q = _db.UserSystemAccesses.Where(a => a.userprofileid == userId && a.status >= 0);
            return q.ForEachAsync(a => a.status = -1, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}