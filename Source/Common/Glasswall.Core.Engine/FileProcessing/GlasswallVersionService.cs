using System;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;

namespace Glasswall.Core.Engine.FileProcessing
{
    public class GlasswallVersionService : IGlasswallVersionService
    {
        private readonly IGlasswallFileOperations _glasswallFileOperations;

        public GlasswallVersionService(IGlasswallFileOperations glasswallFileOperations)
        {
            _glasswallFileOperations = glasswallFileOperations ?? throw new ArgumentNullException(nameof(glasswallFileOperations));
        }

        public string GetVersion()
        {
            return _glasswallFileOperations.GetLibraryVersion();
        }
    }
}
