﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DiseaseService.Models
{
    public class Pesticide : DateBaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [StringLength(100000)]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        [AllowNull]
        public ICollection<PesticideImages> PesticideImages { get; set; }

        public ICollection<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
    }
}
