﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DiseaseService.DTOs
{
    public class PreventativeMeasureDTO
    {
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        [StringLength(10000)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
