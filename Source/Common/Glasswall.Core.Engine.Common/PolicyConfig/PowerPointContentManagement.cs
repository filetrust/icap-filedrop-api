using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class PowerPointContentManagement : ContentManagementFlagsBase
    {
        public ContentManagementFlagAction? Macros { get; set; }
        public ContentManagementFlagAction? ReviewComments { get; set; }
    }
}
