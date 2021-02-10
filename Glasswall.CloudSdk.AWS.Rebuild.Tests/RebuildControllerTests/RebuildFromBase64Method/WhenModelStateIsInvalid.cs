using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromBase64Method
{
    [TestFixture]
    public class WhenModelStateIsInvalid : RebuildControllerTestBase
    {
        private IActionResult _result;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            CommonSetup();

            ClassInTest.ModelState.AddModelError("SomeError", "SomeMessage");

            _result = (IActionResult)ClassInTest.RebuildFromBase64(null);
        }

        [Test]
        public void Bad_Request_Is_Returned()
        {
            Assert.That(_result, Is.Not.Null);
            Assert.That(_result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public void Bad_Request_Contains_Errors()
        {
            var result = _result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<SerializableError>());
        }
        
        [Test]
        public void Error_Is_Expected()
        {
            var result = _result as BadRequestObjectResult;
            var responseBody = (SerializableError)result?.Value;
            Assert.That(responseBody, Has.One.With.Property("Key").EqualTo("SomeError"));
            Assert.That(responseBody, Has.One.With.Property("Value").Contains("SomeMessage"));
        }
    }
}