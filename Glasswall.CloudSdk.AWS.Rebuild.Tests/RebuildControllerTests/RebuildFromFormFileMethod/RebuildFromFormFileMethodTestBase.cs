using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Glasswall.CloudSdk.AWS.Rebuild.Tests.RebuildControllerTests.RebuildFromFormFileMethod
{
    public class RebuildFromFormFileMethodTestBase : RebuildControllerTestBase
    {
        protected const string ExpectedFileName = "Some Expected";
        protected byte[] ValidFileBytes;

        protected Mock<IFormFile> ValidFormFileMock;
        protected Mock<IFormFile> InvalidFormFileMock;
        
        protected override void CommonSetup()
        {
            base.CommonSetup();

            ValidFormFileMock = new Mock<IFormFile>();
            ValidFileBytes = new byte[] {0xDE, 0xAD, 0xBE, 0xEF};
            InvalidFormFileMock = new Mock<IFormFile>();

            ValidFormFileMock.Setup(s => s.FileName)
                .Returns(ExpectedFileName);

            ValidFormFileMock.Setup(s => s.CopyTo(It.IsAny<Stream>()))
                    .Callback((Stream ms) =>
                    {
                        using var source = new MemoryStream(ValidFileBytes);
                        source.CopyTo(ms);
                    });

            ValidFormFileMock.Setup(s => s.Length).Returns(ValidFileBytes.Length);
        }
    }
}
