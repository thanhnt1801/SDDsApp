using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.AspNetCore.Http;

namespace WebApplicationClient.DTOs
{
    public class PredictionDTO
    {
        public long Id { get; set; }
        public long DiseaseId { get; set; }
        public Guid FarmerId { get; set; }
        public Guid ExpertId { get; set; }
        [Required]
        public IFormFile InputImagePath { get; set; }
        [Required]
        public IFormFile OutputImage { get; set; }
        [StringLength(255)]
        public string PredictResult { get; set; }
        [StringLength(20)]
        public string ExpertConfirmation { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        [StringLength(20)]
        public string PredictionPercent { get; set; }
    }
}
