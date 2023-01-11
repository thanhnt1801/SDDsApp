namespace WebApplicationClient.Models
{
    public class SymptomImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Symptom Symptom { get; set; }
        public long SymptomId { get; set; }
    }
}
