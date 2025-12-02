using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Interfaces;

namespace Service.BackgroundService
{
    public class SubscriptionResetBackgroundService(
        ILogger<SubscriptionResetBackgroundService> logger,
        IServiceProvider serviceProvider)
        : Microsoft.Extensions.Hosting.BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Subscription Reset Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Running subscription reset at {Time}", DateTime.UtcNow);

                    using var scope = serviceProvider.CreateScope();
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                    // ✅ GỌI METHOD TỪ SERVICE
                    await subscriptionService.ResetExpiredSubscriptionsAsync();

                    logger.LogInformation("Subscription reset completed");

                    // Run every 1 hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while resetting subscriptions");
                    // Retry after 5 minutes if error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
