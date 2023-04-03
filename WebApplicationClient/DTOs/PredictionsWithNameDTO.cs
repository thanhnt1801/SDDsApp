using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;

namespace WebApplicationClient.DTOs
{
    public class PredictionsWithNameDTO
    {
        public long Id { get; set; }
        public long DiseaseId { get; set; }
        public string FarmerName { get; set; }
        public string ExpertName { get; set; }
        [Required]
        public string InputImagePath { get; set; }
        [Required]
        public string OutputImage { get; set; }
        [StringLength(300)]
        public string PredictResult { get; set; }
        [StringLength(20)]
        public string ExpertConfirmation { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        [StringLength(50)]
        public double PredictionPercent { get; set; }
    }
}
