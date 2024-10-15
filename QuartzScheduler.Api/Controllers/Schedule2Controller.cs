using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuartzScheduler.Api.Models;
using QuartzScheduler.Api.Models.Version2;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace QuartzScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/schedule")]
    [ApiVersion("2")]
    public class Schedule2Controller : ControllerBase
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly IMapper _mapper;
        private readonly IScheduleObjectService _scheduleObjectService;

        public Schedule2Controller(
            ILogger<ScheduleController> logger,
            IMapper mapper,
            IScheduleObjectService scheduleObjectService)
        {
            _logger = logger;
            _mapper = mapper;
            _scheduleObjectService = scheduleObjectService;
        }

        [SwaggerResponse(200)]
        [HttpGet("exists/{appId}/{id}")]
        public async Task<ActionResult<bool>> Exists([FromRoute] Guid appId, [FromRoute] string id, CancellationToken cancellationToken)
        {
            var result = await _scheduleObjectService.DoesScheduleExistsAsync(appId, id, cancellationToken);

            return Ok(result);
        }

        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [HttpGet("{appId}/{id}")]
        public async Task<GetScheduleResponse> Get([FromRoute] Guid appId, [FromRoute] string id,  CancellationToken cancellationToken)
        {
            var result = await _scheduleObjectService.GetScheduleDetailsAsync(appId, id, cancellationToken);

            return _mapper.Map<GetScheduleResponse>(result);
        }

        [SwaggerResponse(201)]
        [SwaggerResponse(400)]
        [HttpPost("{appId}/{id}")]
        public async Task<IActionResult> Post([FromRoute] Guid appId, [FromRoute] string id, [FromBody] ScheduleCreationV2Request request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Create schedule task request: cron expression {request.CronExpression} start time {request.StartDateTime}, end time {request.EndDateTime} time zone {request.TimeZoneOffset}");

            await _scheduleObjectService.CreateScheduleObjectAsync(appId, id,
                _mapper.Map<ScheduleCreationModel<HttpNotificationModel>>(request), cancellationToken);

            return Created(GenerateGetUri(appId, id), null);
        }

        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [HttpDelete("{appId}/{id}")]
        public async Task Delete([FromRoute] Guid appId, [FromRoute] string id, CancellationToken cancellationToken)
        {
            await _scheduleObjectService.DeleteScheduleAsync(appId, id, cancellationToken);
        }

        private string GenerateGetUri(Guid appId, string id)
        {
            var path = $"api/v2/schedule/{appId}/{id}";

            try
            {
                if (Request.PathBase.HasValue && !string.IsNullOrEmpty(Request.PathBase.Value))
                    return new Uri(new Uri(Request.PathBase.Value), path).ToString();

                var builder = new UriBuilder(Request.Scheme, Request.Host.Host);
                if (Request.Host.Port.HasValue)
                    builder.Port = Request.Host.Port.Value;
                builder.Path = path;
                return builder.Uri.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
