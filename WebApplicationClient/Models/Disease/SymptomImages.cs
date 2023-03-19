namespace WebApplicationClient.Models.Disease
{
    public class SymptomImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Symptom Symptom { get; set; }
        public long SymptomId { get; set; }
        public bool Status { get; set; } = true;
    }
}
