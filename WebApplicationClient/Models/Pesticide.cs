using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationClient.Models
{
    public class Pesticide : DateBaseEntity
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public string Image { get; set; }
    }
}
