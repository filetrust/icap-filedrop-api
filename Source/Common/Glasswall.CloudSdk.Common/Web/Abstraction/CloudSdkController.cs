using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.Common.Web.Abstraction
{
    [Route("api/[controller]")]
    public abstract class CloudSdkController<TController> : ControllerBase
    {
        protected readonly Stopwatch TimeMetricTracker = new Stopwatch();
        protected readonly IMetricService MetricService;
        protected readonly ILogger<TController> Logger;

        protected CloudSdkController(
            ILogger<TController> logger,
            IMetricService metricService)
        {
            MetricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected bool TryGetBase64File(string base64File, out byte[] file)
        {
            file = null;

            TimeMetricTracker.Restart();

            try
            {
                file = Convert.FromBase64String(base64File);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Could not parse base64 file {0}", base64File);
            }

            var fileSize = file?.Length ?? 0;
            MetricService.Record(Metric.Base64DecodeTime, TimeMetricTracker.Elapsed);
            MetricService.Record(Metric.FileSize, fileSize);
            return fileSize > 0;
        }

        protected bool TryReadFormFile(IFormFile formFile, out byte[] file)
        {
            file = null;

            TimeMetricTracker.Restart();

            try
            {
                using var ms = new MemoryStream();
                formFile.CopyTo(ms);
                file = ms.ToArray();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Could not parse input file.");
            }

            TimeMetricTracker.Stop();

            var fileSize = formFile?.Length ?? 0;
            MetricService.Record(Metric.FormFileReadTime, TimeMetricTracker.Elapsed);
            MetricService.Record(Metric.FileSize, fileSize);
            return file?.Length > 0;
        }

        protected bool TryGetFile(Uri url, out byte[] file)
        {
            file = null;

            TimeMetricTracker.Restart();

            try
            {
                file = GetFileAsync(url).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError("Could not download file from Url {0} - Exception: {1}", url, ex.ToString());
            }

            TimeMetricTracker.Stop();

            var fileSize = file?.Length ?? 0;
            MetricService.Record(Metric.DownloadTime, TimeMetricTracker.Elapsed);
            MetricService.Record(Metric.FileSize, fileSize);
            return fileSize > 0;
        }

        protected bool TryPutFile(Uri url, byte[] file)
        {
            bool success;
            
            TimeMetricTracker.Restart();

            try
            {
                var response = PutFileAsync(url, file).GetAwaiter().GetResult();

                MetricService.Record(Metric.UploadEtag, response.Headers?.ETag?.Tag);

                success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError("Could not put file to Url {0} - Exception: {1}", url, ex.ToString());
                success = false;
            }

            TimeMetricTracker.Stop();
            MetricService.Record(Metric.UploadSize, file.Length);
            MetricService.Record(Metric.UploadTime, TimeMetricTracker.Elapsed);
            return success;
        }

        private static async Task<byte[]> GetFileAsync(Uri url)
        {
            var request = new FlurlRequest(url);
            var response = await request.SendAsync(HttpMethod.Get);

            return await response.Content.ReadAsByteArrayAsync();
        }
        
        private static async Task<HttpResponseMessage> PutFileAsync(Uri url, byte[] file)
        {
            var request = new FlurlRequest(url);
            var response = await request.SendAsync(HttpMethod.Put, new ByteArrayContent(file));

            return response;
        }
    }
}
