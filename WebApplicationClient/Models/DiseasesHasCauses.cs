namespace WebApplicationClient.Models
{
    public class DiseasesHasCauses
    {
        public long DiseaseId { get; set; }
        public Disease Disease { get; set; }

        public long CauseId { get; set; }
        public Cause Cause { get; set; }
    }
}
