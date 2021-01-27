using System;
using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;
using Glasswall.Core.Engine.Messaging;
using Microsoft.Extensions.Logging;

namespace Glasswall.Core.Engine
{
    public class GlasswallCoreEngine : IGlasswallCoreEngine
    {
        private readonly IGlasswallFileOperations _glasswallFileOperations;
        private readonly ILogger<GlasswallCoreEngine> _logger;

        public GlasswallCoreEngine(IGlasswallFileOperations glasswallFileOperations, ILogger<GlasswallCoreEngine> logger)
        {
            _glasswallFileOperations = glasswallFileOperations ?? throw new ArgumentNullException(nameof(glasswallFileOperations));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetLibraryVersion()
        {
            return _glasswallFileOperations.GetLibraryVersion();
        }

        public FileType DetermineFileType(byte[] fileData)
        {
            return _glasswallFileOperations.DetermineFileType(fileData);
        }

        public EngineOutcome GetConfiguration(out string configuration)
        {
            return _glasswallFileOperations.GetConfiguration(out configuration);
        }

        public EngineOutcome SetConfiguration(string configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return _glasswallFileOperations.SetConfiguration(configuration);
        }

        public EngineOutcome AnalyseFile(byte[] fileContent, string fileType, out string analysisReport)
        {
            if (fileContent == null) throw new ArgumentNullException(nameof(fileContent));
            if (fileType == null) throw new ArgumentNullException(nameof(fileType));

            return _glasswallFileOperations.AnalyseFile(fileContent, fileType, out analysisReport);
        }

        public EngineOutcome ProtectFile(byte[] fileContent, string fileType, out byte[] protectedFile)
        {
            if (fileContent == null) throw new ArgumentNullException(nameof(fileContent));
            if (fileType == null) throw new ArgumentNullException(nameof(fileType));

            return _glasswallFileOperations.ProtectFile(fileContent, fileType, out protectedFile);
        }

        public string GetErrorMessage()
        {
            return _glasswallFileOperations.GetErrorMessage();
        }

        public EngineOutcome GetThreatCensorsAttributes(out string threats)
        {
            return _glasswallFileOperations.GetThreatCensorsAttributes(out threats);
        }
    }
}