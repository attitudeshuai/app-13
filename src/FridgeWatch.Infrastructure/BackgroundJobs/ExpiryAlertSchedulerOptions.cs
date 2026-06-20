namespace FridgeWatch.Infrastructure.BackgroundJobs;

public class ExpiryAlertSchedulerOptions
{
    public const string SectionName = "ExpiryAlertScheduler";

    public bool Enabled { get; set; } = true;

    public int RunAtHour { get; set; } = 0;

    public int RunAtMinute { get; set; } = 0;

    public bool RunOnStartup { get; set; } = false;

    public int StartupDelaySeconds { get; set; } = 30;
}
