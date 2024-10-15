using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace QuartzScheduler.Api.Workers
{
    public class QuartzSchedulerHostedService : IHostedService
    {
        private readonly ILogger<QuartzSchedulerHostedService> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;

        public QuartzSchedulerHostedService(
            ILogger<QuartzSchedulerHostedService> logger,
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
        }

        public IScheduler Scheduler { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(QuartzSchedulerHostedService)} start");

            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            await Scheduler.Start(cancellationToken);

            _logger.LogInformation("Scheduler started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
            
            _logger.LogInformation("Scheduler stopped");
            _logger.LogInformation($"{nameof(QuartzSchedulerHostedService)} stopped");
        }
    }
}
