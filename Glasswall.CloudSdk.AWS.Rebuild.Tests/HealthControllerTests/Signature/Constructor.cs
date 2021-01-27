using System;
using Glasswall.CloudSdk.AWS.Rebuild.Controllers;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.HealthControllerTests.Signature
{
    [TestFixture]
    public class Constructor
    {
        [Test]
        public void Valid_Arguments_Should_Construct()
        {
            var controller = new HealthController(
                Mock.Of<ILogger<HealthController>>(),
                Mock.Of<IGlasswallVersionService>(),
                Mock.Of<IMetricService>());

            Assert.That(controller, Is.Not.Null);
        }

        [Test]
        public void Null_Logger_Should_Throw()
        {
            Assert.That(() => new HealthController(
                    null,
                    Mock.Of<IGlasswallVersionService>(),
                    Mock.Of<IMetricService>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("logger"));
        }

        [Test]
        public void Null_VersionService_Should_Throw()
        {
            Assert.That(() => new HealthController(
                    Mock.Of<ILogger<HealthController>>(),
                    null,
                    Mock.Of<IMetricService>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("glasswallVersionService"));
        }

        [Test]
        public void Null_Metric_Service_Should_Throw()
        {
            Assert.That(() => new HealthController(
                    Mock.Of<ILogger<HealthController>>(),
                    Mock.Of<IGlasswallVersionService>(),
                    null),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("metricService"));
        }
    }
}