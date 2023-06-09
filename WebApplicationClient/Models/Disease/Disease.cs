﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationClient.Models.Disease
{
    public class Disease : DateBaseEntity
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [MaxLength(10000, ErrorMessage = "The description is to long!")]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<DiseaseImages> DiseaseImages { get; set; }

        public ICollection<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
        public ICollection<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
        public ICollection<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }
    }
}
