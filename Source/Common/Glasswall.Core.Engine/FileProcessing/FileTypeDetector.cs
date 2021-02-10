using System;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;
using Glasswall.Core.Engine.Messaging;
using Microsoft.Extensions.Logging;

namespace Glasswall.Core.Engine.FileProcessing
{
    public class FileTypeDetector : IFileTypeDetector
    {
        private readonly IGlasswallFileOperations _glasswallFileOperations;
        private readonly ILogger<FileTypeDetector> _logger;

        public FileTypeDetector(IGlasswallFileOperations glasswallFileOperations, ILogger<FileTypeDetector> logger)
        {
            _glasswallFileOperations = glasswallFileOperations ?? throw new ArgumentNullException(nameof(glasswallFileOperations));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public FileTypeDetectionResponse DetermineFileType(byte[] fileBytes)
        {
            if (fileBytes == null) throw new ArgumentNullException(nameof(fileBytes));

            var fileType = FileType.Unknown;

            try
            {
                fileType = _glasswallFileOperations.DetermineFileType(fileBytes);

                if (!Enum.IsDefined(typeof(FileType), fileType))
                {
                    _logger.Log(LogLevel.Warning, $"The value of 'fileType' {(int)fileType} is invalid, fallback to {FileType.Unknown}");

                    fileType = FileType.Unknown;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning, 0, $"Defaulting 'FileType' to {FileType.Unknown} due to {e.Message}");
            }

            return new FileTypeDetectionResponse(fileType);
        }
    }
}