namespace CajValp.Models
{
    public class FtpControlRecord
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public DateTime ImportDate { get; set; }
        public bool IsProcessed { get; set; }
    }
}