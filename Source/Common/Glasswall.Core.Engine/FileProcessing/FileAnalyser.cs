using System;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Microsoft.Extensions.Logging;

namespace Glasswall.Core.Engine.FileProcessing
{
    public class FileAnalyser : IFileAnalyser
    {
        private const string ActionLabel = "FileAnalyser";

        private readonly IAdaptor<ContentManagementFlags, string> _glasswallConfigurationAdaptor;

        private readonly IGlasswallFileOperations _glasswallCoreEngine;
        private readonly ILogger<FileAnalyser> _logger;
        private readonly IGlasswallEngineSemaphore _glasswallEngineSemaphore;

        public FileAnalyser(IGlasswallFileOperations glasswallCoreEngine, IAdaptor<ContentManagementFlags, string> glasswallConfigurationAdaptor, ILogger<FileAnalyser> logger,
            IGlasswallEngineSemaphore glasswallEngineSemaphore)
        {
            _glasswallCoreEngine = glasswallCoreEngine ?? throw new ArgumentNullException(nameof(glasswallCoreEngine));
            _glasswallConfigurationAdaptor = glasswallConfigurationAdaptor ?? throw new ArgumentNullException(nameof(glasswallConfigurationAdaptor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _glasswallEngineSemaphore = glasswallEngineSemaphore ?? throw new ArgumentNullException(nameof(glasswallEngineSemaphore));
        }

        public async Task<string> GetReportAsync(ContentManagementFlags flags, string fileType, byte[] fileContent, CancellationToken cancellationToken)
        {
            var analysisReport = string.Empty;
            
            var glasswallConfiguration = _glasswallConfigurationAdaptor.Adapt(flags);

            if (glasswallConfiguration == null)
            {
                _logger.Log(LogLevel.Error, 0, "Error processing configuration as it is null");
                return analysisReport;
            }

            await _glasswallEngineSemaphore.Manage(() =>
            {
                var setConfigurationEngineOutcome = _glasswallCoreEngine.SetConfiguration(glasswallConfiguration);
                if (setConfigurationEngineOutcome != EngineOutcome.Success)
                {
                    _logger.Log(LogLevel.Error, 0, $"Error processing configuration '{glasswallConfiguration}'. The GW Engine outcome was '{setConfigurationEngineOutcome}'");
                    return Task.CompletedTask;
                }

                var engineOutcome = _glasswallCoreEngine.AnalyseFile(fileContent, fileType, out analysisReport);
                if (engineOutcome != EngineOutcome.Success)
                {
                    var libraryError = _glasswallCoreEngine.GetErrorMessage();
                    _logger.Log(engineOutcome == EngineOutcome.InternalError ? LogLevel.Error : LogLevel.Information,
                        0,
                        $"Core Engine Error '{engineOutcome:G}' {libraryError ?? string.Empty} whilst analysing file of type '{fileType}'.");
                }
                return Task.CompletedTask;
            }, cancellationToken);

            return analysisReport;
        }
    }
}
