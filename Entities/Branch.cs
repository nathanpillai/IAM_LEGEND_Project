using System.ComponentModel.DataAnnotations.Schema;

namespace IAMLegend.Entities
{
    [Table("branch")]
    public class Branch
    {
        public string branchcode { get; set; } = default!;
        public string branchname { get; set; } = default!;
    }
}