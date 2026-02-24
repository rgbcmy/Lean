using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebUI.Core.Health;

/// <summary>
/// Disk space health check
/// </summary>
public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly long _minimumFreeBytesThreshold;

    public DiskSpaceHealthCheck(long minimumFreeBytesInGB = 1)
    {
        _minimumFreeBytesThreshold = minimumFreeBytesInGB * 1024 * 1024 * 1024; // Convert GB to bytes
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");

            var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            var totalSpaceGB = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var usedSpaceGB = totalSpaceGB - freeSpaceGB;
            var usagePercentage = (usedSpaceGB / totalSpaceGB) * 100;

            var data = new Dictionary<string, object>
            {
                { "driveName", driveInfo.Name },
                { "freeSpaceGB", Math.Round(freeSpaceGB, 2) },
                { "totalSpaceGB", Math.Round(totalSpaceGB, 2) },
                { "usedSpaceGB", Math.Round(usedSpaceGB, 2) },
                { "usagePercentage", Math.Round(usagePercentage, 2) }
            };

            if (driveInfo.AvailableFreeSpace < _minimumFreeBytesThreshold)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"磁盘空间不足 (剩余: {Math.Round(freeSpaceGB, 2)} GB)",
                    data: data));
            }

            if (usagePercentage > 90)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"磁盘使用率过高 ({Math.Round(usagePercentage, 2)}%)",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"磁盘空间充足 (剩余: {Math.Round(freeSpaceGB, 2)} GB)",
                data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "磁盘空间检查失败",
                ex));
        }
    }
}

/// <summary>
/// Memory health check
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _thresholdInBytes;

    public MemoryHealthCheck(long thresholdInMB = 512)
    {
        _thresholdInBytes = thresholdInMB * 1024 * 1024;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allocated = GC.GetTotalMemory(false);
            var allocatedMB = allocated / (1024.0 * 1024.0);

            var data = new Dictionary<string, object>
            {
                { "allocatedMB", Math.Round(allocatedMB, 2) },
                { "gen0Collections", GC.CollectionCount(0) },
                { "gen1Collections", GC.CollectionCount(1) },
                { "gen2Collections", GC.CollectionCount(2) }
            };

            if (allocated > _thresholdInBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"内存使用量较高 ({Math.Round(allocatedMB, 2)} MB)",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"内存使用正常 ({Math.Round(allocatedMB, 2)} MB)",
                data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "内存检查失败",
                ex));
        }
    }
}

/// <summary>
/// Readiness health check - verifies system is ready to accept traffic
/// </summary>
public class ReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Basic readiness check
        // Add more specific checks as needed based on your requirements
        try
        {
            // Check if application is fully started
            // In a real implementation, you would check:
            // - Database migrations are applied
            // - Required external services are accessible
            // - Cache is initialized
            // - Configuration is valid
            
            return Task.FromResult(HealthCheckResult.Healthy("系统已就绪，可以接受请求"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "就绪性检查失败",
                ex));
        }
    }
}
