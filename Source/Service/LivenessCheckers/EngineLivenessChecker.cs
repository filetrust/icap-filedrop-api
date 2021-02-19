using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.Rebuild.LivenessCheckers
{
    public class EngineLivenessChecker : IHealthCheck
    {
        private readonly ILogger<EngineLivenessChecker> _logger;
        private readonly IEngineHealthWatcher _engineHealthWatcher;

        public EngineLivenessChecker(
            IEngineHealthWatcher engineHealthWatcher,
            ILogger<EngineLivenessChecker> logger)
        {
            _engineHealthWatcher = engineHealthWatcher ?? throw new ArgumentNullException(nameof(engineHealthWatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var currentHealthStatus = _engineHealthWatcher.EngineTimeout ? HealthStatus.Unhealthy : HealthStatus.Healthy;

            _logger.Log(LogLevel.Trace, 0, $"Reporting {currentHealthStatus}");

            return Task.FromResult(new HealthCheckResult(currentHealthStatus));
        }
    }
}