using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public DateTime LogTime { get; set; } = DateTime.UtcNow;

        [StringLength(150)]
        public string? Username { get; set; }

        [StringLength(250)]
        public string? Action { get; set; }

        public string? Details { get; set; }
    }
}
