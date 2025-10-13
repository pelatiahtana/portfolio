using System;

public class WagonDetachmentLog
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
    public string WagonId { get; set; }
    public string Details { get; set; }
    public string UserName { get; set; }
    public string ReportedBy { get; set; }
    public string Severity { get; set; }
    public string Status { get; set; }
    public string Actions { get; set; }
    public string Location { get; set; }
    public string TrainId { get; set; }
}