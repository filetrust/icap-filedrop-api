using System;
using Glasswall.Core.Engine.Common;
using NUnit.Framework;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromFormFileMethod
{
    [TestFixture]
    public class WhenEngineThrows : RebuildFromFormFileMethodTestBase
    {
        private Exception _dummyException;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            CommonSetup();
            
            GlasswallVersionServiceMock.Setup(s => s.GetVersion())
                .Throws(_dummyException = new Exception());
        }

        [Test]
        public void Exception_Is_Rethrown()
        {
            Assert.That(() => ClassInTest.RebuildFromFormFile(Newtonsoft.Json.JsonConvert.SerializeObject(Policy.DefaultContentManagementFlags), ValidFormFileMock.Object),
                Throws.Exception.EqualTo(_dummyException));
        }
    }
}