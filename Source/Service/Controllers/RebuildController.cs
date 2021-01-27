using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Abstraction;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.Messaging;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.Rebuild.Controllers
{
    public class RebuildController : CloudSdkController<RebuildController>
    {
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IFileTypeDetector _fileTypeDetector;
        private readonly IFileProtector _fileProtector;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public RebuildController(
            IGlasswallVersionService glasswallVersionService,
            IFileTypeDetector fileTypeDetector,
            IFileProtector fileProtector,
            IMetricService metricService,
            ILogger<RebuildController> logger,
            IWebHostEnvironment hostingEnvironment) : base(logger, metricService)
        {
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
            _fileProtector = fileProtector ?? throw new ArgumentNullException(nameof(fileProtector));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        [HttpPost("file")]
        public async Task<IActionResult> RebuildFromFormFile([FromForm]string contentManagementFlagJson, [FromForm][Required]IFormFile file)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out var fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                var fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    contentManagementFlags, fileType.FileTypeName, fileBytes)); 

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                return new FileContentResult(protectedFileResponse.ProtectedFile, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        [HttpPost("zipfile")]
        public async Task<IActionResult> RebuildFromFormZipFile([FromForm] string contentManagementFlagJson, [FromForm][Required] IFormFile file)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormZipFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out var fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                ExtractZipFile(zipFilePath, null, zipFolderPath);
                foreach (var extractedFile in Directory.GetFiles(zipFolderPath))
                {
                    using FileStream stream = System.IO.File.OpenRead(extractedFile);
                    IFormFile iFormFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (!TryReadFormFile(iFormFile, out fileBytes))
                        return BadRequest("Input file could not be read.");

                    fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                    if (fileType.FileType == FileType.Unknown)
                        return UnprocessableEntity("Input file could not be processed.");

                    IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    contentManagementFlags, fileType.FileTypeName, fileBytes));

                    if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                    {
                        if (protectedFileResponse.IsDisallowed)
                            return Ok(protectedFileResponse);

                        return UnprocessableEntity(
                            $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                    }

                    System.IO.File.WriteAllBytes(Path.Combine(protectedZipFolderPath, Path.GetFileName(extractedFile)), protectedFileResponse.ProtectedFile);
                }

                CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                byte[] protectedZipBytes = System.IO.File.ReadAllBytes($"{protectedZipFolderPath}.{FileType.Zip}");
                return new FileContentResult(protectedZipBytes, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("s3tozip")]
        public async Task<IActionResult> RebuildFromFormS3ToZipFile([FromForm] string contentManagementFlagJson, [FromForm][Required] string presignedURL)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormS3ToZipFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                AmazonS3Client amazonS3Client = new AmazonS3Client(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), RegionEndpoint.EUWest1);

                AmazonS3Uri amazonS3Uri = new AmazonS3Uri(presignedURL);
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = amazonS3Uri.Bucket,
                    Key = amazonS3Uri.Key
                };

                GetObjectResponse s3objectResponse = await amazonS3Client.GetObjectAsync(request);

                MemoryStream memStream = new MemoryStream();
                s3objectResponse.ResponseStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                IFormFile file = new FormFile(memStream, 0, memStream.Length, null, Path.GetFileName(amazonS3Uri.Key));
                if (!TryReadFormFile(file, out var fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                ExtractZipFile(zipFilePath, null, zipFolderPath);
                foreach (var directory in Directory.EnumerateDirectories(zipFolderPath))
                {
                    if (directory.EndsWith("__MACOSX"))
                    {
                        continue;
                    }

                    foreach (var extractedFile in Directory.GetFiles(directory))
                    {
                        using FileStream stream = System.IO.File.OpenRead(extractedFile);
                        IFormFile iFormFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                        if (!TryReadFormFile(iFormFile, out fileBytes))
                            return BadRequest("Input file could not be read.");

                        fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                        if (fileType.FileType == FileType.Unknown)
                            return UnprocessableEntity("Input file could not be processed.");

                        IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                        contentManagementFlags, fileType.FileTypeName, fileBytes));

                        if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                        {
                            if (protectedFileResponse.IsDisallowed)
                                return Ok(protectedFileResponse);

                            return UnprocessableEntity(
                                $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                        }

                        System.IO.File.WriteAllBytes(Path.Combine(protectedZipFolderPath, Path.GetFileName(extractedFile)), protectedFileResponse.ProtectedFile);
                    }
                }

                CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                byte[] protectedZipBytes = System.IO.File.ReadAllBytes($"{protectedZipFolderPath}.{FileType.Zip}");
                await memStream.DisposeAsync();
                return new FileContentResult(protectedZipBytes, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("s3tos3")]
        public async Task<IActionResult> RebuildFromFormS3ToS3([FromForm] string contentManagementFlagJson, [FromForm][Required] string sourcePresignedURL, [FromForm][Required] string targetPresignedURL)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormS3ToS3));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                AmazonS3Client amazonS3Client = new AmazonS3Client(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), RegionEndpoint.EUWest1);
                AmazonS3Uri amazonS3Uri = new AmazonS3Uri(sourcePresignedURL);
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = amazonS3Uri.Bucket,
                    Key = amazonS3Uri.Key
                };

                GetObjectResponse s3objectResponse = await amazonS3Client.GetObjectAsync(request);

                MemoryStream memStream = new MemoryStream();
                s3objectResponse.ResponseStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                IFormFile file = new FormFile(memStream, 0, memStream.Length, null, Path.GetFileName(amazonS3Uri.Key));
                if (!TryReadFormFile(file, out var fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                ExtractZipFile(zipFilePath, null, zipFolderPath);
                foreach (var directory in Directory.EnumerateDirectories(zipFolderPath))
                {
                    if (directory.EndsWith("__MACOSX"))
                    {
                        continue;
                    }

                    foreach (var extractedFile in Directory.GetFiles(directory))
                    {
                        using FileStream stream = System.IO.File.OpenRead(extractedFile);
                        IFormFile iFormFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                        if (!TryReadFormFile(iFormFile, out fileBytes))
                            return BadRequest("Input file could not be read.");

                        fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                        if (fileType.FileType == FileType.Unknown)
                            return UnprocessableEntity("Input file could not be processed.");

                        IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                        contentManagementFlags, fileType.FileTypeName, fileBytes));

                        if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                        {
                            if (protectedFileResponse.IsDisallowed)
                                return Ok(protectedFileResponse);

                            return UnprocessableEntity(
                                $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                        }

                        System.IO.File.WriteAllBytes(Path.Combine(protectedZipFolderPath, Path.GetFileName(extractedFile)), protectedFileResponse.ProtectedFile);
                    }
                }

                CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                using (Stream fs = System.IO.File.OpenRead($"{protectedZipFolderPath}.{FileType.Zip}"))
                {
                    AmazonS3Uri amazonS3TargetUri = new AmazonS3Uri(targetPresignedURL);
                    PutObjectRequest putRequest = new PutObjectRequest()
                    {
                        InputStream = fs,
                        BucketName = amazonS3TargetUri.Bucket,
                        Key = amazonS3TargetUri.Key
                    };

                    PutObjectResponse response = await amazonS3Client.PutObjectAsync(putRequest);
                    await memStream.DisposeAsync();
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return BadRequest("S3 target updation failed.");
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("base64")]
        public async Task<IActionResult> RebuildFromBase64([FromBody][Required]Base64Request request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromBase64));

                if (!ModelState.IsValid) 
                    return BadRequest(ModelState);
                
                if (!TryGetBase64File(request.Base64, out var file))
                    return BadRequest("Input file could not be decoded from base64.");

                RecordEngineVersion();

                var fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file));

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                return Ok(Convert.ToBase64String(protectedFileResponse.ProtectedFile));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> RebuildUrlToUrl([FromBody][Required] UrlToUrlRequest request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildUrlToUrl));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetFile(request.InputGetUrl, out var file))
                    return BadRequest("Input file could not be downloaded.");

                RecordEngineVersion();

                var fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file));

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                if (!TryPutFile(request.OutputPutUrl, protectedFileResponse.ProtectedFile))
                    return BadRequest("Could not put protected file to the supplied output url");

                return Ok();
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

        private FileTypeDetectionResponse DetectFromBytes(byte[] bytes)
        {
            TimeMetricTracker.Restart();
            var fileTypeResponse = _fileTypeDetector.DetermineFileType(bytes);
            TimeMetricTracker.Stop();
            
            MetricService.Record(Metric.DetectFileTypeTime, TimeMetricTracker.Elapsed);
            return fileTypeResponse;
        }

        private IFileProtectResponse RebuildFromBytes(ContentManagementFlags contentManagementFlags, string fileType, byte[] bytes)
        {
            contentManagementFlags = contentManagementFlags.ValidatedOrDefault();

            TimeMetricTracker.Restart();
            var response = _fileProtector.GetProtectedFile(contentManagementFlags, fileType, bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.RebuildTime, TimeMetricTracker.Elapsed);
            return response;
        }

        private void ExtractZipFile(string archivePath, string password, string outFolder)
        {

            using (Stream fsInput = System.IO.File.OpenRead(archivePath))
            using (var zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(fsInput))
            {

                if (!String.IsNullOrEmpty(password))
                {
                    // AES encrypted entries are handled automatically
                    zf.Password = password;
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:
                    //entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here
                    // to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // 4K is optimum
                    var buffer = new byte[4096];

                    // Unzip file in buffered chunks. This is just as fast as unpacking
                    // to a buffer the full size of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var zipStream = zf.GetInputStream(zipEntry))
                    using (Stream fsOutput = System.IO.File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, fsOutput, buffer);
                    }
                }
            }
        }

        private void CreateZipFile(string outPathname, string password, string folderName)
        {
            using (FileStream fsOut = System.IO.File.Create(outPathname))
            using (var zipStream = new ZipOutputStream(fsOut))
            {
                //0-9, 9 being the highest level of compression
                zipStream.SetLevel(3);

                // optional. Null is the same as not setting. Required if using AES.
                zipStream.Password = password;

                // This setting will strip the leading part of the folder path in the entries, 
                // to make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign to 0.
                int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

                CompressFolder(folderName, zipStream, folderOffset);
            }

        }

        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            var files = Directory.GetFiles(path);

            foreach (var filename in files)
            {
                var fi = new FileInfo(filename);

                // Make the name in zip based on the folder
                var entryName = filename.Substring(folderOffset);

                // Remove drive from name and fix slash direction
                entryName = ZipEntry.CleanName(entryName);

                var newEntry = new ZipEntry(entryName);

                // Note the zip format stores 2 second granularity
                newEntry.DateTime = fi.LastWriteTime;

                // Specifying the AESKeySize triggers AES encryption. 
                // Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003,
                // WinZip 8, Java, and other older code, you need to do one of the following: 
                // Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, 
                // you do not need either, but the zip will be in Zip64 format which
                // not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                var buffer = new byte[4096];
                using (FileStream fsInput = System.IO.File.OpenRead(filename))
                {
                    StreamUtils.Copy(fsInput, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            // Recursively call CompressFolder on all folders in path
            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
    }
}
