using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.FileProcessing;

namespace Glasswall.Core.Engine.FileProcessing
{
    public class FileProtectResponse : IFileProtectResponse
    {
        public byte[] ProtectedFile { get; set; }

        public EngineOutcome Outcome { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsDisallowed
        {
            get
            {
                var lower = ErrorMessage.ToLower();
                return lower.Contains("disallow") || lower.Contains("forbidden");
            }
        }
    }
}