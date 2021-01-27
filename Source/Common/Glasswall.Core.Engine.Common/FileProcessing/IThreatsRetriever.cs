using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IThreatsRetriever
    {
        Task<string> GetThreatAttributesInfo(CancellationToken token);
    }
}