using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallEngineSemaphore
    {
        Task Manage(Func<Task> runnableTask, CancellationToken cancellationToken);
    }
}