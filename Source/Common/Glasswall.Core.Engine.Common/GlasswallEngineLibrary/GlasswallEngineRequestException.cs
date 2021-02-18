using System;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public class GlasswallEngineRequestException
        : Exception
    {
        public GlasswallEngineRequestException(Exception exception)
            : base("The engine request threw an exception", exception)
        {
            switch (exception)
            {
                case TaskCanceledException _:
                    RequestState = GlasswallEngineRequestState.CoreEngineCallTimedOut;
                    break;
                case OperationCanceledException _:
                    RequestState = GlasswallEngineRequestState.SemaphoreWaitTimeout;
                    break;
                default:
                    RequestState = GlasswallEngineRequestState.Error;
                    break;
            }
        }

        public GlasswallEngineRequestState RequestState { get; }
    }

    public enum GlasswallEngineRequestState
    {
        CoreEngineCallTimedOut,
        SemaphoreWaitTimeout,
        Error
    }
}