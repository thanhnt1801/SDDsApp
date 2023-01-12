using System.ComponentModel.DataAnnotations;

namespace DiseaseService.Models
{
    public class PreventativeMeasureImages
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public PreventativeMeasure PreventativeMeasure { get; set; }
        public long PreventativeMeasureId { get; set; }
        public bool Status { get; set; } = true;
    }
}
