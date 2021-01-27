using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IFileAnalyser
    {
        string GetReport(
            ContentManagementFlags flags,
            string fileType, 
            byte[] fileBytes);
    }
}
