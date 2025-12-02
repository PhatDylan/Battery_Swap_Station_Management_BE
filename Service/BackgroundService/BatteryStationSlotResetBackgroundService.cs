using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
namespace Service.BackgroundService;

public class BatteryStationResetBackgroundService(
    ILogger<BatteryStationResetBackgroundService> logger,
        IServiceProvider serviceProvider
) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       logger.LogInformation("Battery Station Slot Reset Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Running battery station slot reset at {Time}", DateTime.UtcNow);

                    using var scope = serviceProvider.CreateScope();
                    var stationBatterySlotService = scope.ServiceProvider.GetRequiredService<IStationBatterySlotService>();
                    
                    await stationBatterySlotService.ResetStationBatterySlot();

                    logger.LogInformation("Battery station slot reset completed");

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while resetting battery station slots");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
    }
}