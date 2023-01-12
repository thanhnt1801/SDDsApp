namespace WebApplicationClient.Models
{
    public class CauseImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Cause Cause { get; set; }
        public long CauseId { get; set; }
        public bool Status { get; set; } = true;
    }
}
