using Glasswall.CloudSdk.AWS.Rebuild.Controllers;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.HealthControllerTests.GetHealthMethod
{
    [TestFixture]
    public class WhenRequestingHealth
    {
        private IActionResult _result;
        private HealthController _controllerInTest;
        private Mock<ILogger<HealthController>> _loggerMock;
        private Mock<IGlasswallVersionService> _versionMock;
        private Mock<IMetricService> _metricService;
        private string _expectedVersion;

        [OneTimeSetUp]
        public void Setup()
        {
            _metricService = new Mock<IMetricService>();
            _loggerMock = new Mock<ILogger<HealthController>>();
            _versionMock = new Mock<IGlasswallVersionService>();

            _controllerInTest = new HealthController(
                _loggerMock.Object,
                _versionMock.Object,
                _metricService.Object);

            _versionMock.Setup(s => s.GetVersion()).Returns(_expectedVersion = "banana");

            _result = _controllerInTest.GetHealth();
        }

        [Test]
        public void Version_Is_Retrieved()
        {
            _versionMock.Verify(s => s.GetVersion(), Times.Once);
            _versionMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Version_Is_Recorded()
        {
            _metricService.Verify(s => s.Record(Metric.Version, It.Is<string>(x => x == _expectedVersion)));
            _metricService.VerifyNoOtherCalls();
        }

        [Test]
        public void Ok_Is_Returned()
        {
            Assert.That(_result, Is.InstanceOf<OkResult>());
        }
    }
}
