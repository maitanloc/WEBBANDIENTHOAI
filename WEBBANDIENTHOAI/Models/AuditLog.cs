using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; } // (LogId)

        public DateTime LogTime { get; set; } = DateTime.UtcNow; // (LogTime)

        [MaxLength(150)]
        public string Username { get; set; } // (Username)

        [MaxLength(250)]
        public string Action { get; set; } // (Action)

        public string Details { get; set; } // (Details)
    }
}
