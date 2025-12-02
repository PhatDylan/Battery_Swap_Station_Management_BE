using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("PasswordResetToken")]
    public class PasswordResetToken
    {
        [Key]
        [Column("reset_id")]
        public string ResetId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("otp_hash")]
        [StringLength(255)]
        public string OtpHash { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

        [Column("consumed")]
        public bool Consumed { get; set; } = false;

        [Column("attempt_count")]
        public int AttemptCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
