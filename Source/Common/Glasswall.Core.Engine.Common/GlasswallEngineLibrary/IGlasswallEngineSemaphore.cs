using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallEngineSemaphore
    {
        Task Manage(Func<Task> runnableTask, CancellationToken cancellationToken);
    }

    public class GlasswallEngineSemaphore : IGlasswallEngineSemaphore, IDisposable
    {
        private readonly IGlasswallEngineSemaphoreExceptionOptions _exceptionOptions;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1);

        public GlasswallEngineSemaphore(IGlasswallEngineSemaphoreExceptionOptions exceptionOptions)
        {
            _exceptionOptions = exceptionOptions ?? throw new ArgumentNullException(nameof(exceptionOptions));
        }

        public async Task Manage(Func<Task> runnableTask, CancellationToken cancellationToken)
        {
            if (runnableTask == null) throw new ArgumentNullException(nameof(runnableTask));

            var suppressRelease = false;

            try
            {
                await _connectionLock.WaitAsync(cancellationToken);
                await runnableTask().ContinueWith(Continuation, cancellationToken);
            }
            catch (Exception ex)
            {
                suppressRelease = _exceptionOptions.ReleaseRequiresSuppression(ex);

                throw new GlasswallEngineRequestException(ex);
            }
            finally
            {
                if (!suppressRelease)
                    _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _connectionLock?.Dispose();
        }

        private static void Continuation(Task t)
        {
            if (t.Exception != null) throw t.Exception;
        }
    }
}