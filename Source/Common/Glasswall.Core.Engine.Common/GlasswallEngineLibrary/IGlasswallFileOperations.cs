using Glasswall.Core.Engine.Messaging;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallFileOperations
    {
        string GetLibraryVersion();
        string GetEngineError();

        FileType DetermineFileType(byte[] fileData);

        EngineOutcome GetConfiguration(out string configuration);
        EngineOutcome SetConfiguration(string configuration);

        EngineOutcome AnalyseFile(byte[] fileContent, string fileType, out string analysisReport);
        EngineOutcome ProtectFile(byte[] fileContent, string fileType, out byte[] protectedFile);
        
        EngineOutcome GetThreatCensorsAttributes(out string threats);

        string GetErrorMessage();
    }
}