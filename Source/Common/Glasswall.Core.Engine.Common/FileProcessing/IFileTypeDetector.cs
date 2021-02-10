using System.Threading;
using Glasswall.Core.Engine.Messaging;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IFileTypeDetector
    {
        FileTypeDetectionResponse DetermineFileType(
            byte[] fileBytes);
    }
}
