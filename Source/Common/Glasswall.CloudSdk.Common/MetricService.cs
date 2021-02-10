using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.Common
{
    public class MetricService : IMetricService
    {
        private readonly ILogger<MetricService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MetricService(
            ILogger<MetricService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Record<TMetric>(string metricName, TMetric value)
        {
            var responseHeaders = _httpContextAccessor.HttpContext.Response.Headers;

            if (responseHeaders.ContainsKey(metricName))
                return;

            _logger.LogTrace($"Setting header '{metricName}' to value '{value}'");
            responseHeaders.Add(metricName, value?.ToString() ?? "");
        }
    }
}