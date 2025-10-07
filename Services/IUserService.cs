using IAMLegend.Dtos;
using IAMLegend.Entities;
using IAMLegend.Models;

namespace IAMLegend.Services
{

    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
        Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CreateUserRequest req, string createdBy, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateUserRequest req, string modifiedBy, CancellationToken ct = default);
        Task SoftDeleteUserProfileAsync(int id, string modifiedBy, CancellationToken ct = default);
        Task UpdateUserPermissionsAsync(UserProfileEditViewModel model, string modifiedBy, CancellationToken ct = default);
        Task<UserProfileEditViewModel> GetUserProfileWithPermissionsAsync(int userId, CancellationToken ct = default);
        Task<UserProfileEditViewModel> GetBlankUserProfileWithPermissionsAsync(CancellationToken ct = default);
        Task CreateUserAsync(UserProfileEditViewModel model, string createdBy, CancellationToken ct = default);
    }
}