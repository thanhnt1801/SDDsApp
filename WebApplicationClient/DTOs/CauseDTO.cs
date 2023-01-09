using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.DTOs
{
    public class CauseDTO
    {
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        [StringLength(2000)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public IFormFile Image { get; set; }
    }
}
