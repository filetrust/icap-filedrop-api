using System;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromFormFileMethod
{
    [TestFixture]
    public class WhenFormFileIsInvalid : RebuildFromFormFileMethodTestBase
    {
        private IActionResult _result;

        [SetUp]
        public void OnetimeSetup()
        {
            CommonSetup();

            _result = (IActionResult)ClassInTest.RebuildFromFormFile(null, InvalidFormFileMock.Object);
        }

        [Test]
        public void Bad_Request_Is_Returned()
        {
            _result = (IActionResult)ClassInTest.RebuildFromFormFile(null, InvalidFormFileMock.Object);

            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<BadRequestObjectResult>());
            var result = _result as BadRequestObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<string>());
            Assert.That(result.Value, Is.EqualTo("Input file could not be read."));
        }

        [Test]
        public void Metrics_Are_Recorded()
        {

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.FormFileReadTime),
                        It.Is<TimeSpan>(x => x > TimeSpan.Zero)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.FileSize),
                        It.Is<long>(x => x == 0)),
                Times.Once);

            MetricServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void No_Engine_Actions_Are_Performed()
        {
            _result = (IActionResult)ClassInTest.RebuildFromFormFile(null, InvalidFormFileMock.Object);

            GlasswallVersionServiceMock.VerifyNoOtherCalls();
            FileTypeDetectorMock.VerifyNoOtherCalls();
            FileProtectorMock.VerifyNoOtherCalls();
        }
    }
}