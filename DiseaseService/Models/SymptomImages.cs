using System.ComponentModel.DataAnnotations;

namespace DiseaseService.Models
{
    public class SymptomImages
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Symptom Symptom { get; set; }
        public long SymptomId { get; set; }
    }
}
