using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApp.Models
{
    [Table("Report")]
    public class Report
    {
        [Key]
        [Column("Report_ID")]
        public int ReportId { get; set; }

        [Column("Admin_ID")]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        [StringLength(50)]
        [Column("Report_Type")]
        public string? ReportType { get; set; }

        [Column("Generated_Date")]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        [Column("File_Path")]
        public string? FilePath { get; set; }

        // Navigation property
        public virtual Admin Admin { get; set; } = null!;
    }
}