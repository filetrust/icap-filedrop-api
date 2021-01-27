using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class ContentManagementFlags
    {
        public PdfContentManagement PdfContentManagement { get; set; }
        public ExcelContentManagement ExcelContentManagement { get; set; }
        public PowerPointContentManagement PowerPointContentManagement { get; set; }
        public WordContentManagement WordContentManagement { get; set; }
        public TiffContentManagement TiffContentManagement { get; set; }
    }
}