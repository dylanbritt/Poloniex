using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poloniex.Core.Domain.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required, MaxLength(32)]
        public string UserName { get; set; }

        [Required, MaxLength(32)]
        public string Password { get; set; }

        [Required, MaxLength(32)]
        public string EncryptedPassword { get; set; }

        [Required, MaxLength(32)]
        public string Salt { get; set; }
    }
}
