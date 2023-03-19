using System.Collections.Generic;

namespace WebApplicationClient.Models.Disease
{
    public class Symptom : DateBaseEntity
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; } = true;

        public ICollection<SymptomImages> SymptomImages { get; set; }
    }
}
