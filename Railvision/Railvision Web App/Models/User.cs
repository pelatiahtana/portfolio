using System.ComponentModel.DataAnnotations;

namespace TrainGenie.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Role { get; set; } // "Admin" or "Operator"

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}