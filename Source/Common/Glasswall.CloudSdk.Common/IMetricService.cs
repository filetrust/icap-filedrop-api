using System;

namespace Glasswall.CloudSdk.Common
{
    public interface IMetricService
    {
        void Record<TMetric>(string metricName, TMetric value);
    }
}