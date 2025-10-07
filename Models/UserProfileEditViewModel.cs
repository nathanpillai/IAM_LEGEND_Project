using IAMLegend.Entities;

namespace IAMLegend.Models
{
    public class UserProfileEditViewModel
    {
        required public UserProfile UserProfile { get; set; }

        public List<PermissionRowViewModel> PermissionsRows { get; set; } = new List<PermissionRowViewModel>();
    }
}