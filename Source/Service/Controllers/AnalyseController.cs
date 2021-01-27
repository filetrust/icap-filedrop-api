using System;
using System.Threading.Tasks;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Abstraction;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.Analyse.Controllers
{
    public class AnalyseController : CloudSdkController<AnalyseController>
    {
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IFileTypeDetector _fileTypeDetector;
        private readonly IFileAnalyser _fileAnalyser;

        public AnalyseController(
            IGlasswallVersionService glasswallVersionService,
            IFileTypeDetector fileTypeDetector,
            IFileAnalyser fileAnalyser,
            IMetricService metricService,
            ILogger<AnalyseController> logger) : base(logger, metricService)
        {
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
            _fileAnalyser = fileAnalyser ?? throw new ArgumentNullException(nameof(fileAnalyser));
        }

        [HttpPost("base64")]
        public async Task<IActionResult> AnalyseFromBase64([FromBody]Base64Request request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(AnalyseFromBase64));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetBase64File(request.Base64, out var file))
                    return BadRequest("Input file could not be decoded from base64.");

                await Task.Run(() => RecordEngineVersion());

                var fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var xmlReport = await Task.Run(() => AnalyseFromBytes(request.ContentManagementFlags, fileType.FileTypeName, file));

                if (string.IsNullOrWhiteSpace(xmlReport))
                    return UnprocessableEntity("No report could be generated for file.");

                return Ok(xmlReport);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        [HttpPost("url")]
        public async Task<IActionResult> AnalyseFromUrl([FromBody] UrlRequest request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(AnalyseFromBase64));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetFile(request.InputGetUrl, out var file))
                    return BadRequest("Input file could not be downloaded.");

                await Task.Run(() => RecordEngineVersion());

                var fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var xmlReport = await Task.Run(() => AnalyseFromBytes(request.ContentManagementFlags, fileType.FileTypeName, file));

                if (string.IsNullOrWhiteSpace(xmlReport))
                    return UnprocessableEntity("No report could be generated for file.");

                return Ok(xmlReport);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        private string AnalyseFromBytes(ContentManagementFlags contentManagementFlags, string fileType, byte[] bytes)
        {
            contentManagementFlags = contentManagementFlags.ValidatedOrDefault();

            TimeMetricTracker.Restart();
            var response = _fileAnalyser.GetReport(contentManagementFlags, fileType, bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.AnalyseTime, TimeMetricTracker.Elapsed);
            return response;
        }
        
        private void RecordEngineVersion()
        {
            var version = _glasswallVersionService.GetVersion();
            MetricService.Record(Metric.Version, version);
        }

        private FileTypeDetectionResponse DetectFromBytes(byte[] bytes)
        {
            TimeMetricTracker.Restart();
            var fileTypeResponse = _fileTypeDetector.DetermineFileType(bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.DetectFileTypeTime, TimeMetricTracker.Elapsed);
            return fileTypeResponse;
        }

    }
}