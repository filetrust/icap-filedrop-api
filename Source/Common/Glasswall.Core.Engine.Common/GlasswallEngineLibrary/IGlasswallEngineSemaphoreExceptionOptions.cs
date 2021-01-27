using System;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallEngineSemaphoreExceptionOptions
    {
        bool ReleaseRequiresSuppression(Exception exception);
    }
}