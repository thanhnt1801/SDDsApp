namespace WebApplicationClient.Models
{
    public class PreventativeMeasureImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public PreventativeMeasure PreventativeMeasure { get; set; }
        public long PreventativeMeasureId { get; set; }
    }
}
