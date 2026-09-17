namespace WeddingPlanner.Core.Domain.Entities;

public class Timeline
{
    public int Id { get; set; }
    public Wedding Wedding { get; set; } = new Wedding();
    public string EventName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;
    public string Description { get; set; } = string.Empty;
}