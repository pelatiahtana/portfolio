namespace TrainGenie.Models
{
    public class Incident
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportTime { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public string AdminNotes { get; set; }
        public string TrainId { get; set; } // <-- Add this
    }
}