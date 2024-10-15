using QuartzScheduler.Common.Models;

namespace QuartzScheduler.Common.Validators
{
    public interface ICronModelValidator
    {
        void CheckIfCronModelIsValid<T>(ScheduleCreationModel<T> model);
    }
}
