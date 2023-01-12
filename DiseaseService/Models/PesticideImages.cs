using System.ComponentModel.DataAnnotations;

namespace DiseaseService.Models
{
    public class PesticideImages
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Pesticide Pesticide { get; set; }
        public long PesticideId { get; set; }
        public bool Status { get; set; } = true;
    }
}
