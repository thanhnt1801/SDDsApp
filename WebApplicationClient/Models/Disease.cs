using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationClient.Models
{
    public class Disease : DateBaseEntity
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public string Image { get; set; } = string.Empty;

        public ICollection<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
        public ICollection<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
        public ICollection<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }
    }
}
