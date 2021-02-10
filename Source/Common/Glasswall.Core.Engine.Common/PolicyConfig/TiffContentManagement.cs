using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class TiffContentManagement : ContentManagementFlagsBase
    {
        public ContentManagementFlagAction? Geotiff { get; set; }
    }
}