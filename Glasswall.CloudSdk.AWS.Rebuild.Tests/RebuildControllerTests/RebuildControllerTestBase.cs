using Flurl.Http.Testing;
using Glasswall.CloudSdk.AWS.Rebuild.Controllers;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests
{
    public class RebuildControllerTestBase
    {
        /// <summary>
        /// For mocking URL to url rebuild GET/PUT
        /// </summary>
        protected HttpTest HttpTest;

        protected RebuildController ClassInTest;
        protected Mock<IGlasswallVersionService> GlasswallVersionServiceMock;
        protected Mock<IFileTypeDetector> FileTypeDetectorMock;
        protected Mock<IFileProtector> FileProtectorMock;
        protected Mock<IMetricService> MetricServiceMock;
        protected Mock<ILogger<RebuildController>> LoggerMock;
        protected Mock<IWebHostEnvironment> HostingEnvironmentMock;

        protected virtual void CommonSetup()
        {
            GlasswallVersionServiceMock = new Mock<IGlasswallVersionService>();
            FileTypeDetectorMock = new Mock<IFileTypeDetector>();
            FileProtectorMock = new Mock<IFileProtector>();
            MetricServiceMock = new Mock<IMetricService>();
            LoggerMock = new Mock<ILogger<RebuildController>>();
            HostingEnvironmentMock = new Mock<IWebHostEnvironment>();

            ClassInTest = new RebuildController(
                GlasswallVersionServiceMock.Object,
                FileTypeDetectorMock.Object,
                FileProtectorMock.Object,
                MetricServiceMock.Object,
                LoggerMock.Object,
                HostingEnvironmentMock.Object
            );

            HttpTest = new HttpTest();
        }
    }
}