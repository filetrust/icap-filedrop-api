using System;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Abstraction;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.FileTypeDetection.Controllers
{
    public class FileTypeDetectionController : CloudSdkController<FileTypeDetectionController>
    {
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IFileTypeDetector _fileTypeDetector;

        public FileTypeDetectionController(
            IGlasswallVersionService glasswallVersionService,
            IFileTypeDetector fileTypeDetector,
            IMetricService metricService,
            ILogger<FileTypeDetectionController> logger) : base(logger, metricService)
        {
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
        }

        [HttpPost("base64")]
        public async Task<IActionResult> DetermineFileTypeFromBase64([FromBody]Base64Request request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(DetermineFileTypeFromBase64));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetBase64File(request.Base64, out var file))
                    return BadRequest("Input file could not be decoded from base64.");

                await Task.Run(RecordEngineVersion, cancellationToken);

                var fileType = await DetectFromBytes(file, cancellationToken);

                return Ok(fileType);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        [HttpPost("url")]
        public async Task<IActionResult> DetermineFileTypeFromUrl([FromBody] UrlRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("{0} method invoked", nameof(DetermineFileTypeFromUrl));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetFile(request.InputGetUrl, out var file))
                    return BadRequest("Input file could not be downloaded.");

                await Task.Run(RecordEngineVersion);

                var fileType = await DetectFromBytes(file, cancellationToken);

                return Ok(fileType);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        private void RecordEngineVersion()
        {
            var version = _glasswallVersionService.GetVersion();
            MetricService.Record(Metric.Version, version);
        }

        private async Task<FileTypeDetectionResponse> DetectFromBytes(byte[] bytes, CancellationToken cancellationToken)
        {
            TimeMetricTracker.Restart();
            var fileTypeResponse = await _fileTypeDetector.DetermineFileTypeAsync(bytes, cancellationToken);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.DetectFileTypeTime, TimeMetricTracker.Elapsed);
            return fileTypeResponse;
        }
    }
}
