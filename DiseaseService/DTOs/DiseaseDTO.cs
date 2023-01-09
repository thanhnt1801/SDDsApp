using System;
using System.ComponentModel.DataAnnotations;

namespace DiseaseService.DTOs
{
    public class DiseaseDTO
    {
        public long Id { get; set; }
        [Required]
        [StringLength(191)]
        public string Name { get; set; }
        public bool Status { get; set; } = true;
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
