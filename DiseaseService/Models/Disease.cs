using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DiseaseService.Models
{
    public class Disease : DateBaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(191)]
        public string Name { get; set; }
        [StringLength(100000)]
        [DataType(DataType.Text)] 
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        [AllowNull]
        public ICollection<DiseaseImages> DiseaseImages { get; set; }

        public ICollection<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
        public ICollection<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
        public ICollection<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }    
        public ICollection<DiseasesHasCauses> DiseasesHasCauses { get; set; }    
    }
}
