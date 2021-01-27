using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface ILibraryVersionDetector
    {
        string UnknownVersion { get; }
    
        Task<string> GetVersionAsync(CancellationToken token);
    }
}