using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgePMO.API.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string Token { get; set; }

        [Required]
        public DateTime Expiration { get; set; }

        public bool IsUsed { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
