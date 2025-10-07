using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{
    [Table("domain")]
    public class Domain
    {
        public int domainid { get; set; }
        public string domainname { get; set; } = default!;
    }
}