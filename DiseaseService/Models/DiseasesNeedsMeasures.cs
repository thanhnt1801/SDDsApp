namespace DiseaseService.Models
{
    public class DiseasesNeedsMeasures
    {
        public long DiseaseId { get; set; }
        public Disease Disease { get; set; }

        public long PreventativeMeasureId { get; set; }
        public PreventativeMeasure PreventativeMeasure { get; set; }
    }
}
