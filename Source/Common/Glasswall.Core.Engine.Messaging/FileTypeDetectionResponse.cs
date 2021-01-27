using System;

namespace Glasswall.Core.Engine.Messaging
{
    [Serializable]
    public class FileTypeDetectionResponse
    {
        public FileTypeDetectionResponse(FileType fileType)
        {
            FileType = fileType;
        }

        public FileType FileType { get; }

        public string FileTypeName => FileType.ToString();
    }
}
