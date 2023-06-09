﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationClient.Models.Disease
{
    public class PreventativeMeasure : DateBaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        [StringLength(191)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<PreventativeMeasureImages> PreventativeMeasureImages { get; set; }
        public ICollection<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }
    }
}
