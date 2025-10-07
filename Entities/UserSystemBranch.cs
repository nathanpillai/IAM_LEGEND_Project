using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{
    [Table("usersystembranch")]
    public class UserSystemBranch
    {
        public int usersystembranchid { get; set; }
        public int userprofileid { get; set; }
        public UserProfile? UserProfile { get; set; }
        public int localsystemid { get; set; }
        public string branchcode { get; set; } = default!;
        public int status { get; set; } = 0;
        public DateTime createddatetime { get; set; } = DateTime.UtcNow;
        public string createdby { get; set; } = default!;
    }

}
