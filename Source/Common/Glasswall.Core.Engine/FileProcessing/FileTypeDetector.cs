using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;
using Glasswall.Core.Engine.Messaging;
using Microsoft.Extensions.Logging;

namespace Glasswall.Core.Engine.FileProcessing
{
    public class FileTypeDetector : IFileTypeDetector
    {
        private readonly IGlasswallFileOperations _glasswallFileOperations;
        private readonly IGlasswallEngineSemaphore _glasswallEngineSemaphore;
        private readonly ILogger<FileTypeDetector> _logger;

        public FileTypeDetector(
            IGlasswallFileOperations glasswallFileOperations,
            ILogger<FileTypeDetector> logger,
            IGlasswallEngineSemaphore glasswallEngineSemaphore)
        {
            _glasswallFileOperations = glasswallFileOperations ?? throw new ArgumentNullException(nameof(glasswallFileOperations));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _glasswallEngineSemaphore = glasswallEngineSemaphore ?? throw new ArgumentNullException(nameof(glasswallEngineSemaphore));
        }

        public async Task<FileTypeDetectionResponse> DetermineFileTypeAsync(byte[] fileData, CancellationToken cancellationToken)
        {
            var fileType = FileType.Unknown;

            await _glasswallEngineSemaphore.Manage(() =>
            {
                try
                {
                    if (!fileData.Any()) return Task.CompletedTask;

                    fileType = _glasswallFileOperations.DetermineFileType(fileData);
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Warning, 0, $"Defaulting 'FileType' to {FileType.Unknown} due to {e.Message}");
                    return Task.CompletedTask;
                }
            }, cancellationToken);

            if (!Enum.IsDefined(typeof(FileType), fileType))
            {
                _logger.Log(LogLevel.Warning, 0, $"The value of 'fileType' {(int)fileType} is invalid, fallback to {FileType.Unknown}");
                fileType = FileType.Unknown;
            }

            return new FileTypeDetectionResponse(fileType);
        }
    }
}