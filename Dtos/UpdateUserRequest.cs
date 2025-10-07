namespace IAMLegend.Dtos
{
    public class UpdateUserRequest
    {
        public bool IsAdmin { get; set; }
        public List<SystemPermission> SystemPermissions { get; set; } = new();
        public class SystemPermission
        {
            public int LocalSystemId { get; set; }
            public int PermissionLevelId { get; set; }
            public List<BranchSelection>? Branches { get; set; }
        }
        public class BranchSelection
        {
            public string BranchCode { get; set; } = default!;
            public bool IsSelected { get; set; }
        }
    }

}