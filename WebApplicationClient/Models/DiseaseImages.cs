namespace WebApplicationClient.Models
{
    public class DiseaseImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Disease Disease { get; set; }
        public long DiseaseId { get; set; }
        public bool Status { get; set; } = true;
    }
}
