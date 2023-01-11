﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.DTOs
{
    public class PreventativeMeasureDTO
    {
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        [StringLength(191)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
