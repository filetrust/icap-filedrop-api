using System;

namespace Glasswall.Core.Engine.Common.Configuration
{
    public interface IGlasswallEngineApiConfiguration
    {
        TimeSpan FileProcessingTimeout { get; }
    }
}