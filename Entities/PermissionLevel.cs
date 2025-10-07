using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{
    [Table("permissionlevel")]
    public class PermissionLevel
    {
        public int permissionlevelid { get; set; }
        public string name { get; set; } = default!;
        public int localsystemid { get; set; }
        public LocalSystem? LocalSystem { get; set; }
    }
}
