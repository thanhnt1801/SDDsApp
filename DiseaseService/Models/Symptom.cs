using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiseaseService.Models
{
    public class Symptom : DateBaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [StringLength(2000)]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public string Image { get; set; }

        public ICollection<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
    }
}
