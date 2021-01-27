using System;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.Rebuild.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IMetricService _metricService;

        public HealthController(
            ILogger<HealthController> logger,
            IGlasswallVersionService glasswallVersionService,
            IMetricService metricService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            _logger.Log(LogLevel.Trace, "Performing heartbeat");

            _metricService.Record(Metric.Version, _glasswallVersionService.GetVersion());

            return Ok();
        }
    }
}