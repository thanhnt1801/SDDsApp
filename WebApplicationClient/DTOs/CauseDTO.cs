using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
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
        [StringLength(100000)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public List<IFormFile> Images { get; set; }
    }
}
