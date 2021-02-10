using System;
using System.Net;
using System.Net.Http;
using Flurl.Http.Testing;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildUrlToUrlTests
{
    [TestFixture]
    public class WhenInputUrlCannotBeDownloaded : RebuildControllerTestBase
    {
        private IActionResult _result;
        private Uri _expectedInputUrl;
        private Uri _expectedOutputUrl;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            CommonSetup();
            
            _expectedInputUrl = new Uri("https://www.myfileserver.com/myfile.png");
            _expectedOutputUrl = new Uri("https://www.s3bucket.com/buckets/rebuilt/myfile.png");

            HttpTest.ResponseQueue.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

            _result = (IActionResult)ClassInTest.RebuildUrlToUrl(new UrlToUrlRequest
            {
                InputGetUrl = _expectedInputUrl,
                OutputPutUrl = _expectedOutputUrl
            });
        }

        [OneTimeTearDown]
        public void OnetimeTeardown()
        {
            HttpTest?.Dispose();
        }

        [Test]
        public void Bad_Request_Is_Returned()
        {

            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<BadRequestObjectResult>()
                .With.Property(nameof(BadRequestObjectResult.Value))
                .EqualTo("Input file could not be downloaded."));
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
                        It.Is<int>(x => x == 0)),
                Times.Once);

            MetricServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void No_Engine_Actions_Are_Performed()
        {
            GlasswallVersionServiceMock.VerifyNoOtherCalls();
            FileTypeDetectorMock.VerifyNoOtherCalls();
            FileProtectorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void File_Download_Was_Attempted()
        {
            HttpTest.ShouldHaveMadeACall().Times(1);
            HttpTest.ShouldHaveCalled(_expectedInputUrl.ToString())
                .With(s => s.HttpStatus == HttpStatusCode.InternalServerError)
                .With(s => s.Request.Method == HttpMethod.Get)
                .Times(1);
        }
    }
}