using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuartzScheduler.Dal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Api.Workers
{
    public class JobsToObjectsCleanerWorker : BackgroundService
    {
        private readonly ILogger<JobsToObjectsCleanerWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public JobsToObjectsCleanerWorker(
            ILogger<JobsToObjectsCleanerWorker> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(JobsToObjectsCleanerWorker)} running at: {DateTimeOffset.Now}");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();

                    await dbContext.CleanJobsToObjectsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning JobsToObjects");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
