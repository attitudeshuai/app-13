using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.Infrastructure.BackgroundJobs;

public class ExpiryAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExpiryAlertBackgroundService> _logger;
    private readonly ExpiryAlertSchedulerOptions _options;
    private readonly TimeProvider _timeProvider;

    public ExpiryAlertBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpiryAlertBackgroundService> logger,
        IOptions<ExpiryAlertSchedulerOptions> options,
        TimeProvider? timeProvider = null)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("临期提醒定时任务已禁用，跳过启动");
            return;
        }

        _logger.LogInformation("临期提醒定时任务已启动，每日执行时间：{Hour:D2}:{Minute:D2}",
            _options.RunAtHour, _options.RunAtMinute);

        if (_options.RunOnStartup)
        {
            var delay = TimeSpan.FromSeconds(_options.StartupDelaySeconds);
            _logger.LogInformation("启动时执行已启用，等待 {Delay} 秒后执行首次扫描...", delay.TotalSeconds);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("启动时执行被取消");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await RunScanAsync(stoppingToken);
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateNextRunDelay();
            _logger.LogInformation("下次临期提醒扫描将在 {Delay} 后执行（约 {NextRunTime:yyyy-MM-dd HH:mm:ss} UTC）",
                FormatDelay(delay), _timeProvider.GetUtcNow().Add(delay));

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("临期提醒定时任务等待被取消");
                break;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await RunScanAsync(stoppingToken);
            }
        }

        _logger.LogInformation("临期提醒定时任务已停止");
    }

    private async Task RunScanAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("开始执行每日临期提醒扫描...");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IExpiryAlertSyncService>();

            var result = await syncService.ScanAndSyncAllAlertsAsync();

            _logger.LogInformation(
                "每日临期提醒扫描完成：处理食材 {Total} 个，更新状态 {StatusUpdated} 个，" +
                "新增临期提醒 {NearExpiry} 个，新增过期提醒 {Expired} 个，移除提醒 {Removed} 个，" +
                "总耗时 {Elapsed} 秒",
                result.TotalFoodItemsProcessed,
                result.FoodItemsStatusUpdated,
                result.NearExpiryAlertsCreated,
                result.ExpiredAlertsCreated,
                result.AlertsRemoved,
                (result.ScanEndTime - result.ScanStartTime).TotalSeconds.ToString("F2"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "每日临期提醒扫描执行失败");
        }
    }

    private TimeSpan CalculateNextRunDelay()
    {
        var now = _timeProvider.GetUtcNow();
        var today = now.Date;
        var scheduledTime = today.AddHours(_options.RunAtHour).AddMinutes(_options.RunAtMinute);

        if (scheduledTime <= now)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        var delay = scheduledTime - now;

        if (delay <= TimeSpan.Zero)
        {
            delay = TimeSpan.FromMinutes(1);
        }

        return delay;
    }

    private static string FormatDelay(TimeSpan delay)
    {
        if (delay.TotalDays >= 1)
        {
            return $"{(int)delay.TotalDays}天{delay.Hours}小时{delay.Minutes}分";
        }
        else if (delay.TotalHours >= 1)
        {
            return $"{delay.Hours}小时{delay.Minutes}分{delay.Seconds}秒";
        }
        else
        {
            return $"{delay.Minutes}分{delay.Seconds}秒";
        }
    }
}
