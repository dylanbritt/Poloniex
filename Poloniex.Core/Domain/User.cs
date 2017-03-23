using System.ComponentModel.DataAnnotations;

namespace Poloniex.Core.Domain
{
    public class User
    {
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
