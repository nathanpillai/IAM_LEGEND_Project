using IAMLegend.Entities;

namespace IAMLegend.Repositories
{
    public interface IQueryUserRepository
    {
        Task<List<Domain>> GetDomainsAsync(CancellationToken ct = default);
        Task<List<Branch>> GetBranchesAsync(CancellationToken ct = default);
        Task<List<LocalSystem>> GetLocalSystemsAsync(CancellationToken ct = default);
        Task<List<PermissionLevel>> GetPermissionLevelsBySystemAsync(int systemId, CancellationToken ct = default);
        Task<List<UserProfile>> GetAllUsersAsync(CancellationToken ct = default);
        Task<UserProfile?> GetUserByIdAsync(int id, CancellationToken ct = default);
        Task<UserProfile?> GetUserByDomainUsernameAsync(string domain, string username, CancellationToken ct = default);
        Task<List<UserSystemBranch>> GetUserSystemBranchesAsync(int userId, CancellationToken ct = default);
        Task<List<UserSystemAccess>> GetUserSystemAccessAsync(int userId, CancellationToken ct = default);
    }

}
