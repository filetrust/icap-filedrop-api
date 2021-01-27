namespace Glasswall.Core.Engine.Common
{
    public enum EngineOutcome
    {
        Success = 1,
        Error = 0,
        SuccessDocumentWriteFailure = -1,
        SuccessAnalysisWriteFailure = -2,
        ErrorAnalysisWriteFailure = -3,
        SuccessReportWriteFailure = -4,
        SuccessDocumentReportWriteFailure = -5,
        ErrorReportWriteFailure = -6,
        SuccessAnalysisReportWriteFailure = -7,
        ErrorAnalysisReportWriteFailure = -8,
        InternalError = -9
    }
}
