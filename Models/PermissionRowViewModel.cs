using Microsoft.AspNetCore.Mvc.Rendering;

namespace IAMLegend.Models
{
    public class BranchCheck
    {
        public string BranchName { get; set; } = default!;
        public bool IsChecked { get; set; } = false;
    }

    public class PermissionRowViewModel
    {
        public int LocalSystemId { get; set; }
        required public string LocalSystemName { get; set; }

        // Branch code -> checked/not
        //public Dictionary<string, bool> Branches { get; set; } = new();
        public Dictionary<string, BranchCheck> Branches { get; set; } = new();
        //public Dictionary<string, (string BranchName, bool IsChecked)> Branches { get; set; } = new Dictionary<string, (string BranchName, bool IsChecked)>();

        // Dropdown list of permission levels
        public List<SelectListItem> PermissionLevelList { get; set; } = new();

        public int SelectedPermissionLevelId { get; set; }
    }
}