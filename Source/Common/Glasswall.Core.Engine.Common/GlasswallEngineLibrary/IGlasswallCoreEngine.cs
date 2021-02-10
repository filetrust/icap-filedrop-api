using Glasswall.Core.Engine.Messaging;

namespace Glasswall.Core.Engine.Common.GlasswallEngineLibrary
{
    public interface IGlasswallCoreEngine
    {
        string GetLibraryVersion();

        FileType DetermineFileType(byte[] fileData);

        EngineOutcome GetConfiguration(out string configuration);
        EngineOutcome SetConfiguration(string configuration);

        EngineOutcome AnalyseFile(byte[] fileContent, string fileType, out string analysisReport);
        EngineOutcome ProtectFile(byte[] fileContent, string fileType, out byte[] protectedFile);

        EngineOutcome GetThreatCensorsAttributes(out string threat);
        string GetErrorMessage();
    }
}