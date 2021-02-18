using System.Threading;
using System.Threading.Tasks;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.Core.Engine.Common.FileProcessing
{
    public interface IFileAnalyser
    {
        Task<string> GetReportAsync(ContentManagementFlags flags, string fileType, byte[] fileContent, CancellationToken cancellationToken);
    }
}