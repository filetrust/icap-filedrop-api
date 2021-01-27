using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class ExcelContentManagement : ContentManagementFlagsBase
    {
        public ContentManagementFlagAction? DynamicDataExchange { get; set; }
        public ContentManagementFlagAction? Macros { get; set; }
        public ContentManagementFlagAction? ReviewComments { get; set; }
    }
}
