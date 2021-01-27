using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildUrlToUrlTests
{
    [TestFixture]
    public class WhenFileTypeIsUnknown : RebuildControllerTestBase
    {
        private IActionResult _result;
        private Uri _expectedInputUrl;
        private Uri _expectedOutputUrl;
        private const string Version = "Some Version";
        private static readonly byte[] ExpectedDownloadFile = { 116, 101, 115, 116 };

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            CommonSetup();

            _expectedInputUrl = new Uri("https://www.myfileserver.com/myfile.png");
            _expectedOutputUrl = new Uri("https://www.s3bucket.com/buckets/rebuilt/myfile.png");

            GlasswallVersionServiceMock.Setup(s => s.GetVersion())
                .Returns(Version);

            FileTypeDetectorMock.Setup(s => s.DetermineFileType(It.IsAny<byte[]>()))
                .Returns(new FileTypeDetectionResponse(FileType.Unknown));

            HttpTest.ResponseQueue.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(ExpectedDownloadFile)
            });

            _result = (IActionResult)ClassInTest.RebuildUrlToUrl(new UrlToUrlRequest
            {
                InputGetUrl = _expectedInputUrl,
                OutputPutUrl = _expectedOutputUrl
            });
        }

        [Test]
        public void StatusCode_Is_Unprocessable()
        {
            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<UnprocessableEntityObjectResult>()
                .With.Property(nameof(UnprocessableEntityObjectResult.Value))
                .EqualTo("File could not be determined to be a supported file"));
        }

        [Test]
        public void Metrics_Are_Recorded()
        {
            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.DownloadTime),
                        It.Is<TimeSpan>(x => x > TimeSpan.Zero)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.FileSize),
                        It.Is<int>(x => x == ExpectedDownloadFile.Length)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.Version),
                        It.Is<string>(x => x == Version)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.DetectFileTypeTime),
                        It.Is<TimeSpan>(x => x > TimeSpan.Zero)),
                Times.Once);

            MetricServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Engine_Version_Is_Retrieved()
        {
            GlasswallVersionServiceMock.Verify(s => s.GetVersion(), Times.Once);
            GlasswallVersionServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void FileTypeDetection_Is_Retrieved()
        {
            FileTypeDetectorMock.Verify(s => s.DetermineFileType(It.Is<byte[]>(x => x.SequenceEqual(ExpectedDownloadFile))), Times.Once);
            FileTypeDetectorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void File_Is_Not_Rebuilt()
        {
            FileProtectorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void File_Download_Was_Attempted()
        {
            HttpTest.ShouldHaveMadeACall().Times(1);
            HttpTest.ShouldHaveCalled(_expectedInputUrl.ToString())
                .With(s => s.HttpStatus == HttpStatusCode.OK)
                .With(s => s.Request.Method == HttpMethod.Get)
                .Times(1);
        }
    }
}