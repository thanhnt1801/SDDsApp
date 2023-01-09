using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.DTOs
{
    public class VerifyAccountDTO
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
