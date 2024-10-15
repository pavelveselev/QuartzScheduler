using System.Threading;
using System.Threading.Tasks;
using QuartzScheduler.Common.Models;

namespace QuartzScheduler.Common.Services;

public interface IScheduleJobService
{
    Task<ScheduleDetails> GetJobDetailsAsync(string key, CancellationToken cancellationToken = default);

    Task<string> CreateNotificationJobAsync<T>(ScheduleCreationModel<T> schedule, CancellationToken cancellationToken = default);

    Task DeleteJobAsync(string key, CancellationToken cancellationToken = default);
}
