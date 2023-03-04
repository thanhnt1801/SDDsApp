using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.DTOs
{
    public class DiseaseDTO
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(100000)]
        public string Description { get; set; }
        [Required]
        public bool Status { get; set; } = true;
        public List<IFormFile> Images { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
