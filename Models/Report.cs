using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Project.Models
{
    public class Report
    {
        [Key]
        public int Report_ID { get; set; }

        [ForeignKey("Admin")]
        public int Admin_ID { get; set; }

        [StringLength(50)]
        public string? Report_Type { get; set; }

        public DateTime Generated_Date { get; set; }

        [StringLength(255)]
        public string? File_Path { get; set; }

        public virtual Admin? Admin { get; set; }
    }
}
