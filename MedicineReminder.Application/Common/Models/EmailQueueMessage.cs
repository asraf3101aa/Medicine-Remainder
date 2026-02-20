namespace MedicineReminder.Application.Common.Models;

public class EmailQueueMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 3;
}
