using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{
    [Table("usersystemaccess")]
    public class UserSystemAccess
    {
        public int usersystemaccessid { get; set; }
        public int userprofileid { get; set; }
        public UserProfile? UserProfile { get; set; }
        public int localsystemid { get; set; }
        public int permissionlevelid { get; set; }
        public int status { get; set; } = 0;
        public DateTime createddatetime { get; set; } = DateTime.UtcNow;
        public string createdby { get; set; } = default!;

    }

}
