using System.ComponentModel.DataAnnotations.Schema;


namespace IAMLegend.Entities
{
    [Table("userprofile")]
    public class UserProfile
    {
        public int userprofileid { get; set; }
        public string domain { get; set; } = default!;
        public string firstname { get; set; } = default!;
        public string lastname { get; set; } = default!;
        public string username { get; set; } = default!;
        public string email { get; set; } = default!;
        public bool isadmin { get; set; }
        public int? operatorid { get; set; }
        public int status { get; set; } = 0; // >=0 active, -1 deleted
        public DateTime createddatetime { get; set; } = DateTime.UtcNow;        
        public string createdby { get; set; } = default!;
        public DateTime? modifieddatetime { get; set; }
        public string? modifiedby { get; set; }
        public ICollection<UserSystemAccess> UserSystemAccesses { get; set; } = new List<UserSystemAccess>();
        public ICollection<UserSystemBranch> UserSystemBranches { get; set; } = new List<UserSystemBranch>();
    }

}
