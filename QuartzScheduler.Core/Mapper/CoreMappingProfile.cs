using AutoMapper;
using Quartz;
using QuartzScheduler.Common.Models;
using System.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace QuartzScheduler.Core.Mapper;

public class CoreMappingProfile : Profile
{
    public CoreMappingProfile()
    {
        CreateMap<NotificationModel<HttpNotificationModel>, JobDataMap>()
            .ConvertUsing(new HttpNotificationModelToJobDataConverter());

        CreateMap<JobDataMap, NotificationModel<HttpNotificationModel>>()
            .ConvertUsing(new JobDataToHttpNotificationModelConverter());
    }
}

internal class HttpNotificationFields
{
    public const string Uri = "uri";
    public const string Method = "method";
    public const string Body = "body";
    public const string Headers = "headers";
    public const string Attempts = "attempts";
}

public class HttpNotificationModelToJobDataConverter
    : ITypeConverter<NotificationModel<HttpNotificationModel>, JobDataMap>
{
    public JobDataMap Convert(NotificationModel<HttpNotificationModel> notificationModel,
        JobDataMap destination, ResolutionContext context)
    {
        if (notificationModel.Notification == null)
            throw new ArgumentNullException(nameof(notificationModel.Notification));
        if (string.IsNullOrEmpty(notificationModel.Notification.Uri))
            throw new ArgumentException(nameof(notificationModel.Notification.Uri));
        if (string.IsNullOrEmpty(notificationModel.Notification.Method))
            throw new ArgumentException(nameof(notificationModel.Notification.Method));

        var jobData = new JobDataMap();

        jobData.Put(HttpNotificationFields.Uri, notificationModel.Notification.Uri);
        jobData.Put(HttpNotificationFields.Method, notificationModel.Notification.Method);

        if (!string.IsNullOrEmpty(notificationModel.Notification.Body))
            jobData.Put(HttpNotificationFields.Body, notificationModel.Notification.Body);

        if (notificationModel.Notification.Headers != null && notificationModel.Notification.Headers.Any())
            jobData.Put(HttpNotificationFields.Headers, JsonSerializer.Serialize(notificationModel.Notification.Headers));

        if (notificationModel.Attempts != null)
            jobData.Put(HttpNotificationFields.Attempts, JsonSerializer.Serialize(notificationModel.Attempts));

        return jobData;
    }
}

public class JobDataToHttpNotificationModelConverter
    : ITypeConverter<JobDataMap, NotificationModel<HttpNotificationModel>>
{
    public NotificationModel<HttpNotificationModel> Convert(JobDataMap jobData,
        NotificationModel<HttpNotificationModel> destination, ResolutionContext context)
    {
        if (!jobData.ContainsKey(HttpNotificationFields.Uri))
            throw new ArgumentException("Uri parameter not found in context");

        if (!jobData.ContainsKey(HttpNotificationFields.Method))
            throw new ArgumentException("Method parameter not found in context");

        var headersSerialized = jobData.ContainsKey(HttpNotificationFields.Headers) ? jobData.GetString(HttpNotificationFields.Headers) : null;
        Dictionary<string, string> headers = null;
        try
        {
            headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersSerialized);
        }
        catch (Exception ex)
        {
            //TODO: log
        }

        var attemptsSerialized = jobData.ContainsKey(HttpNotificationFields.Attempts) ? jobData.GetString(HttpNotificationFields.Attempts) : null;
        AttemptsModel attempts = null;
        try
        {
            attempts = JsonSerializer.Deserialize<AttemptsModel>(attemptsSerialized);
        }
        catch (Exception ex)
        {
            //TODO: log
        }

        return new NotificationModel<HttpNotificationModel>
        {
            Notification = new HttpNotificationModel
            {
                Uri = jobData.GetString(HttpNotificationFields.Uri),
                Method = jobData.GetString(HttpNotificationFields.Method),
                Headers = headers,
                Body = jobData.ContainsKey(HttpNotificationFields.Body) ? jobData.GetString(HttpNotificationFields.Body) : null,
            },
            Attempts = attempts
        };
    }
}
