namespace WebApplicationClient.Models.Disease
{
    public class DiseasesNeedsPesticides
    {
        public long DiseaseId { get; set; }
        public Disease Disease { get; set; }

        public long PesticideId { get; set; }
        public Pesticide Pesticide { get; set; }
    }
}
