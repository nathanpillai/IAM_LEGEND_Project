namespace IAMLegend.Dtos
{
    public class CreateUserRequest
    {
        public string domain { get; set; } = default!;
        public string firstname { get; set; } = default!;
        public string lastname { get; set; } = default!;
        public string username { get; set; } = default!;
        public string email { get; set; } = default!;
        public bool isadmin { get; set; }

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
