using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace WebApplicationClient.Models
{
    public class Prediction
    {
        public long Id { get; set; }
        public long DiseaseId { get; set; }
        public Guid FarmerId { get; set; }
        public Guid ExpertId { get; set; }
        [StringLength(100)]
        public string InputImagePath { get; set; }
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
        [AllowNull]
        public string PredictionPercent { get; set; }
        [StringLength(50)]
        [AllowNull]
        public string PredictionBestLabel { get; set; }
        [AllowNull]
        [StringLength(50)]
        public string PredictionMediumLabel { get; set; }
        [AllowNull]
        [StringLength(50)]
        public string PredictionMediumPercent { get; set; }
        [AllowNull]
        [StringLength(50)]
        public string PredictionWorstLabel { get; set; }
        [AllowNull]
        [StringLength(50)]
        public string PredictionWorstPercent { get; set; }
    }
}
