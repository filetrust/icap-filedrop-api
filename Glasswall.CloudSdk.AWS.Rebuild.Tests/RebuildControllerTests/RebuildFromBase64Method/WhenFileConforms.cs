using System;
using System.Linq;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.FileProcessing;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromBase64Method
{
    [TestFixture]
    public class WhenFileConforms : RebuildControllerTestBase
    {
        private const string Version = "Some Version";
        private FileTypeDetectionResponse _expectedType;
        private static readonly byte[] ExpectedDecoded = { 116, 101, 115, 116 };

        private IActionResult _result;

        [OneTimeSetUp]
        public async System.Threading.Tasks.Task OnetimeSetupAsync()
        {
            CommonSetup();

            GlasswallVersionServiceMock.Setup(s => s.GetVersion())
                .Returns(Version);

            FileTypeDetectorMock.Setup(s => s.DetermineFileType(It.IsAny<byte[]>()))
                .Returns(_expectedType = new FileTypeDetectionResponse(FileType.Bmp));

            FileProtectorMock.Setup(s => s.GetProtectedFile(
                    It.IsAny<ContentManagementFlags>(),
                    It.IsAny<string>(),
                    It.IsAny<byte[]>()))
                .Returns(new FileProtectResponse
                {
                    Outcome = EngineOutcome.Success,
                    ProtectedFile = ExpectedDecoded
                });

            _result = await ClassInTest.RebuildFromBase64(new Base64Request
            {
                Base64 = "dGVzdA=="
            });
        }

        [Test]
        public void Ok_Is_Returned()
        {
            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<OkObjectResult>()
                .With.Property(nameof(OkObjectResult.Value))
                .EqualTo(Convert.ToBase64String(ExpectedDecoded)));
        }

        [Test]
        public void Metrics_Are_Recorded()
        {
            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.Base64DecodeTime),
                        It.Is<TimeSpan>(x => x > TimeSpan.Zero)),
                Times.Once);

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.FileSize),
                        It.Is<int>(x => x == ExpectedDecoded.Length)),
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

            MetricServiceMock.Verify(s =>
                    s.Record(
                        It.Is<string>(x => x == Metric.RebuildTime),
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
            FileTypeDetectorMock.Verify(s => s.DetermineFileType(It.Is<byte[]>(x => x.SequenceEqual(ExpectedDecoded))), Times.Once);
            FileTypeDetectorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void File_Is_Rebuilt()
        {
            FileProtectorMock.Verify(
                s => s.GetProtectedFile(
                    It.Is<ContentManagementFlags>(x => x == Policy.DefaultContentManagementFlags),
                    It.Is<string>(x => x == _expectedType.FileTypeName),
                    It.Is<byte[]>(x => x.SequenceEqual(ExpectedDecoded))),
                Times.Once);

            FileProtectorMock.VerifyNoOtherCalls();
        }
    }
}