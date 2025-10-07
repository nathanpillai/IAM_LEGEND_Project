using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{

    [Table("localsystem")]
    public class LocalSystem
    {
        public int localsystemid { get; set; }
        public string localsystemname { get; set; } = default!;
        public ICollection<PermissionLevel> PermissionLevels { get; set; } = new List<PermissionLevel>();
    }

}
