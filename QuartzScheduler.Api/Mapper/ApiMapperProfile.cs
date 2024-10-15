using AutoMapper;
using QuartzScheduler.Api.Models;
using QuartzScheduler.Api.Models.Version2;
using QuartzScheduler.Common.Models;
using System;

namespace QuartzScheduler.Api.Mapper
{
    public class ApiMapperProfile : Profile
    {
        public ApiMapperProfile()
        {
            CreateMap<ScheduleCreationRequest, ScheduleCreationModel<HttpNotificationModel>>()
                .ConvertUsing(new ScheduleCreationRequestConverter());
            CreateMap<ScheduleCreationV2Request, ScheduleCreationModel<HttpNotificationModel>>()
                .ConvertUsing(new ScheduleCreationRequestV2Converter());

            CreateMap<ScheduleDetails, GetScheduleResponse>();
        }
    }

    public class ScheduleCreationRequestConverter : ITypeConverter<ScheduleCreationRequest, ScheduleCreationModel<HttpNotificationModel>>
    {
        public ScheduleCreationModel<HttpNotificationModel> Convert(
            ScheduleCreationRequest source, ScheduleCreationModel<HttpNotificationModel> destination, ResolutionContext context)
        {
            if (source == null)
                return null;

            var result = new ScheduleCreationModel<HttpNotificationModel>
            {
                CronExpression = source.CronExpression,
                StartDateTime = source.StartDateTime,
                EndDateTime = source.EndDateTime,
                TimeZoneOffset = source.TimeZoneOffset,
                NotificationObject = new NotificationModel<HttpNotificationModel>
                {
                    Notification = new HttpNotificationModel
                    {
                        Uri = source.Notification.Uri,
                        Method = source.Notification.Method,
                        Body = source.Notification.Body,
                        Headers = source.Notification.Headers,
                    }
                }
            };
            return result;
        }
    }

    public class ScheduleCreationRequestV2Converter : ITypeConverter<ScheduleCreationV2Request, ScheduleCreationModel<HttpNotificationModel>>
    {
        public ScheduleCreationModel<HttpNotificationModel> Convert(
            ScheduleCreationV2Request source, ScheduleCreationModel<HttpNotificationModel> destination, ResolutionContext context)
        {
            if (source == null)
                return null;

            var result = new ScheduleCreationModel<HttpNotificationModel>
            {
                CronExpression = source.CronExpression,
                StartDateTime = source.StartDateTime,
                EndDateTime = source.EndDateTime,
                TimeZoneOffset = source.TimeZoneOffset,
                
                NotificationObject = new NotificationModel<HttpNotificationModel>
                {
                    Notification = new HttpNotificationModel
                    {
                        Uri = source.Notification.Uri,
                        Method = source.Notification.Method,
                        Body = source.Notification.Body,
                        Headers = source.Notification.Headers,
                    },
                }
            };
            if (source.Attempts != null)
            {
                result.NotificationObject.Attempts = new AttemptsModel
                {
                    CurrentAttempt = 0,
                    Period = TimeSpan.FromSeconds(source.Attempts.PeriodSec),
                    MaxCount = source.Attempts.MaxCount
                };
            }
            return result;
        }
    }
}
