namespace WebApplicationClient.Models
{
    public class PesticideImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Pesticide Pesticide { get; set; }
        public long PesticideId { get; set; }
    }
}
