namespace Glasswall.CloudSdk.AWS.Rebuild.LivenessCheckers
{
    public interface IEngineHealthWatcher
    {
        bool EngineTimeout { get; set; }
    }

    public class EngineHealthWatcher : IEngineHealthWatcher
    {
        public bool EngineTimeout { get; set; }
    }
}