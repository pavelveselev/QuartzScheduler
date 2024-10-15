using System;
using System.Collections.Generic;
using System.Linq;
using QuartzScheduler.Common.Extensions;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Validators;

namespace QuartzScheduler.Core.Validators
{
    public class CronModelValidator : ICronModelValidator
    {
        public void CheckIfCronModelIsValid<T>(ScheduleCreationModel<T> model)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.CronExpression) && !model.StartDateTime.HasValue)
                errors.Add("The crown expression or the time of a single trigger is not specified");
            
            if (model.TimeZoneOffset.HasValue && model.TimeZoneOffset.Value.FindTimeZone() == null)
                errors.Add($"The specified time zone {model.TimeZoneOffset.Value} was not found");

            if (model.NotificationObject == null)
                errors.Add("The notification object is not defined");

            if (model.NotificationObject.Notification == null)
                errors.Add("Не указано уведомление");

            switch (model.NotificationObject.Notification)
            {
                case null:
                    errors.Add("No notification specified");
                    break;
                case HttpNotificationModel notification:
                    if (string.IsNullOrWhiteSpace(notification?.Method))
                        errors.Add("Http notification method is not specified");
                    if (string.IsNullOrWhiteSpace(notification?.Uri))
                        errors.Add("Http notification URI is not specified");
                    break;
                default:
                    break;
            }

            if (errors.Any())
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }
}
