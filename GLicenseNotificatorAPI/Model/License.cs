using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLicenseNotificatorAPI.Model
{
    [Table("License")]
    public class License
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string LicenseNumber { get; set; } = null!;

        public DateTime IsValidUtc { get; set; }

        public bool NotificationSent { get; set; }
    }
}
