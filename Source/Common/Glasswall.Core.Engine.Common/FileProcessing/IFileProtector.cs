using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IFileProtector
    {
        //Task<byte[]> GetProtectedFileAsync(
        //    ContentManagementFlags contentManagementFlags,
        //    string fileType, 
        //    IByteReader byteReader,
        //    CancellationToken cancellationToken);
        IFileProtectResponse GetProtectedFile(ContentManagementFlags contentManagementFlags, string fileType, byte[] fileBytes);
    }
}