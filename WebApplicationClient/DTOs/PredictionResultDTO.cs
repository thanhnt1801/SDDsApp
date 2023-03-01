namespace WebApplicationClient.DTOs
{
    public class PredictionResultDTO
    {
        public double BestProbabiliy { get; set; }
        public double MediumProbabiliy { get; set; }
        public double WorstProbabiliy { get; set; }
        public string BestLabel { get; set; }
        public string MediumLabel { get; set; }
        public string WorstLabel { get; set; }

    }
}
