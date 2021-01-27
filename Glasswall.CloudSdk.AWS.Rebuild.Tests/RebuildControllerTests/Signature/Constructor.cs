using System;
using Glasswall.CloudSdk.AWS.Rebuild.Controllers;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.Signature
{
    [TestFixture]
    public class Constructor
    {
        [Test]
        public void Valid_Arguments_Should_Construct()
        {
            var controller = new RebuildController(
                Mock.Of<IGlasswallVersionService>(),
                Mock.Of<IFileTypeDetector>(),
                Mock.Of<IFileProtector>(),
                Mock.Of<IMetricService>(),
                Mock.Of<ILogger<RebuildController>>(),
                Mock.Of<IWebHostEnvironment>());

            Assert.That(controller, Is.Not.Null);
        }

        [Test]
        public void Null_VersionService_Should_Throw()
        {
            Assert.That(() => new RebuildController(
                    null,
                    Mock.Of<IFileTypeDetector>(),
                    Mock.Of<IFileProtector>(),
                    Mock.Of<IMetricService>(),
                    Mock.Of<ILogger<RebuildController>>(),
                    Mock.Of<IWebHostEnvironment>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("glasswallVersionService"));
        }

        [Test]
        public void Null_Detector_Should_Throw()
        {
            Assert.That(() => new RebuildController(
                    Mock.Of<IGlasswallVersionService>(),
                    null,
                    Mock.Of<IFileProtector>(),
                    Mock.Of<IMetricService>(),
                    Mock.Of<ILogger<RebuildController>>(),
                    Mock.Of<IWebHostEnvironment>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("fileTypeDetector"));
        }

        [Test]
        public void Null_Protector_Should_Throw()
        {
            Assert.That(() => new RebuildController(
                    Mock.Of<IGlasswallVersionService>(),
                    Mock.Of<IFileTypeDetector>(),
                    null,
                    Mock.Of<IMetricService>(),
                    Mock.Of<ILogger<RebuildController>>(),
                    Mock.Of<IWebHostEnvironment>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("fileProtector"));
        }

        [Test]
        public void Null_MetricService_Should_Throw()
        {
            Assert.That(() => new RebuildController(
                    Mock.Of<IGlasswallVersionService>(),
                    Mock.Of<IFileTypeDetector>(),
                    Mock.Of<IFileProtector>(),
                    null,
                    Mock.Of<ILogger<RebuildController>>(),
                    Mock.Of<IWebHostEnvironment>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("metricService"));
        }

        [Test]
        public void Null_Logger_Should_Throw()
        {
            Assert.That(() => new RebuildController(
                    Mock.Of<IGlasswallVersionService>(),
                    Mock.Of<IFileTypeDetector>(),
                    Mock.Of<IFileProtector>(),
                    Mock.Of<IMetricService>(),
                    null,
                    Mock.Of<IWebHostEnvironment>()),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("logger"));
        }
    }
}