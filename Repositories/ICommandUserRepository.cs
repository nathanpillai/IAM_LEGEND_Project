using IAMLegend.Entities;

namespace IAMLegend.Repositories
{
    public interface ICommandUserRepository
    {
        Task<int> InsertUserProfileAsync(UserProfile user, CancellationToken ct = default);
        Task UpdateUserProfileAsync(UserProfile user, CancellationToken ct = default);
        Task SoftDeleteUserProfileAsync(int userId, string modifiedBy, CancellationToken ct = default);
        Task InsertUserSystemBranchAsync(UserSystemBranch branch, CancellationToken ct = default);
        Task InsertUserSystemAccessAsync(UserSystemAccess access, CancellationToken ct = default);
        Task SoftDeleteUserSystemBranchesAsync(int userId, string modifiedBy, CancellationToken ct = default);
        Task SoftDeleteUserSystemAccessAsync(int userId, string modifiedBy, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

}
