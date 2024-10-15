using QuartzScheduler.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Common.Services;

public interface INotificationClient
{
    Task SendAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default);
}
