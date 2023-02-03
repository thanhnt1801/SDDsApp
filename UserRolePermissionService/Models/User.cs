using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        [StringLength(191)]
        [EmailAddress]
        public string Email { get; set; }
        public byte[] passwordHash { get; set; } = new byte[32];
        public byte[] passwordSalt { get; set; } = new byte[32];
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        [RegularExpression("^([0-9]{10})$", ErrorMessage = "Invalid Mobile Number (valid phone number has 10 digits!")]
        public long PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? verificationToken { get; set; }
        public DateTime? verifiedAt { get; set; }
        public string? passwordResetToken { get; set; }
        public DateTime? resetTokenExpires { get; set; }
/*        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenCreated { get; set; }
        public DateTime RefreshTokenExpires { get; set; }*/
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Status { get; set; } = true;

        [ForeignKey("RoleId")]
        public long RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
