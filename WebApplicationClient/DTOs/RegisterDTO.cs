using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(7, ErrorMessage = "{0} must have more than 6 characters")]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [RegularExpression("^([0-9]{10})$", ErrorMessage = "Valid phone number has 10 digits.")]
        [DisplayName("PhoneNumber")]
        public int PhoneNumber { get; set; }

    }
}
