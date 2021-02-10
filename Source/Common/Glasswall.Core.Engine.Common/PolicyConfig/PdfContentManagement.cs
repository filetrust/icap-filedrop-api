using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class PdfContentManagement : ContentManagementFlagsBase
    {
        public ContentManagementFlagAction? Javascript { get; set; }
        public ContentManagementFlagAction? Acroform { get; set; }
        public ContentManagementFlagAction? ActionsAll { get; set; }
        public string Watermark { get; set; }
    }
}
