using QuartzScheduler.Common.Exceptions;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Core.Services
{
    public class HttpNotificationClient : INotificationClient
    {
        public async Task SendAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default)
        {
            var method = notificationModel.Method.ToLower();
            HttpResponseMessage response;
            try
            {
                response = method switch
                {
                    "post" => await SendPostRequestAsync(notificationModel, cancellationToken),
                    "put" => await SendPutRequestAsync(notificationModel, cancellationToken),
                    "get" => await SendGetRequestAsync(notificationModel, cancellationToken),
                    "delete" => await SendDeleteRequestAsync(notificationModel, cancellationToken),
                    _ => throw new NotSupportedException(),
                };
            }
            catch (Exception ex)
            {
                throw new NotificationException($"Error sending request to URL {notificationModel.Uri}: {ex.Message}", ex);
            }

            if (!response.IsSuccessStatusCode)
                throw new NotificationException($"Error sending request to URL {notificationModel.Uri}: Status code {response.StatusCode}");
        }

        private async Task<HttpResponseMessage> SendPostRequestAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return await httpClient.PostAsync(notificationModel.Uri, GenerateContent(notificationModel), cancellationToken);
        }

        private async Task<HttpResponseMessage> SendPutRequestAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return await httpClient.PutAsync(notificationModel.Uri, GenerateContent(notificationModel), cancellationToken);
        }

        private async Task<HttpResponseMessage> SendGetRequestAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return await httpClient.GetAsync(notificationModel.Uri, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendDeleteRequestAsync(HttpNotificationModel notificationModel, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return await httpClient.DeleteAsync(notificationModel.Uri, cancellationToken);
        }

        private static HttpContent GenerateContent(HttpNotificationModel notificationModel)
        {
            var content = new StringContent(notificationModel.Body, Encoding.UTF8, "application/json");

            if (notificationModel.Headers != null && notificationModel.Headers.Any())
                foreach (var header in notificationModel.Headers)
                    content.Headers.Add(header.Key, header.Value);

            return content;
        }
    }
}
