using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuartzScheduler.Api.Models;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/schedule")]
    [ApiVersion("1")]
    public class ScheduleController : ControllerBase
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly IMapper _mapper;
        private readonly IScheduleJobService _scheduleJobService;

        public ScheduleController(
            ILogger<ScheduleController> logger,
            IMapper mapper,
            IScheduleJobService scheduleJobService)
        {
            _logger = logger;
            _mapper = mapper;
            _scheduleJobService = scheduleJobService;
        }

        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [HttpGet("{key}")]
        public async Task<GetScheduleResponse> Get([FromRoute] string key, CancellationToken cancellationToken)
        {
            var result = await _scheduleJobService.GetJobDetailsAsync(key, cancellationToken);

            return _mapper.Map<GetScheduleResponse>(result);
        }

        [HttpPost]
        public async Task<ScheduleCreationResponse> Post([FromBody] ScheduleCreationRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Create schedule task request: cron expression {request.CronExpression} start time {request.StartDateTime}, end time {request.EndDateTime} time zone {request.TimeZoneOffset}");

            try
            {
                var result = await _scheduleJobService.CreateNotificationJobAsync(
                    _mapper.Map<ScheduleCreationModel<HttpNotificationModel>>(request), cancellationToken);

                return new ScheduleCreationResponse {Key = result};
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create scheduleTask: : cron expression {request.CronExpression} start time {request.StartDateTime}, end time {request.EndDateTime} time zone {request.TimeZoneOffset}", ex);
                throw;
            }
        }

        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [HttpDelete("{key}")]
        public async Task Delete([FromRoute] string key, CancellationToken cancellationToken)
        {
            await _scheduleJobService.DeleteJobAsync(key, cancellationToken);
        }
    }
}
