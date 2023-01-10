using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiseaseService.Models
{
    public class CauseImages
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Cause Cause { get; set; }
        public long CauseId { get; set; }
    }
}
