namespace WebApplicationClient.Models.Disease
{
    public class PesticideImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Pesticide Pesticide { get; set; }
        public long PesticideId { get; set; }
        public bool Status { get; set; } = true;
    }
}
