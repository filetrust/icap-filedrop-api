using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallEngineSemaphoreExceptionOptions
    {
        bool ReleaseRequiresSuppression(Exception exception);
    }

    public class GlasswallEngineSemaphoreExceptionOptions : IGlasswallEngineSemaphoreExceptionOptions
    {
        private readonly IEnumerable<Type> _suppressReleaseTypes = GetExceptionTypes();

        private static IEnumerable<Type> GetExceptionTypes()
        {
            return new[]
            {
                typeof(TaskCanceledException),
                typeof(OperationCanceledException)
            };
        }

        public bool ReleaseRequiresSuppression(Exception exception)
        {
            var exceptionType = exception.GetType();
            return _suppressReleaseTypes.Any(s => exceptionType == s);
        }
    }
}