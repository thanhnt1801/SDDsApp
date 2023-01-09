namespace WebApplicationClient.Models
{
    public class DiseasesHasSymptoms
    {
        public long DiseaseId { get; set; }
        public Disease Disease { get; set; }

        public long SymptomId { get; set; }
        public Symptom Symptom { get; set; }
    }
}
