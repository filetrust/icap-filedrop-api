using System.Threading;
using System.Threading.Tasks;
using Glasswall.Core.Engine.Messaging;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IFileTypeDetector
    {
        Task<FileTypeDetectionResponse> DetermineFileTypeAsync(byte[] fileBytes, CancellationToken cancellationToken);
    }
}
