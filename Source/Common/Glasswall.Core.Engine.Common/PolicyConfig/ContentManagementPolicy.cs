using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public class ContentManagementPolicy
    {
        public Guid Id { get; set; }
        public Guid PolicyCatalogueId { get; set; }
        public string Name { get; set; }
        public PdfContentManagement PdfContentManagement { get; set; }
        public ExcelContentManagement ExcelContentManagement { get; set; }
        public PowerPointContentManagement PowerPointContentManagement { get; set; }
        public WordContentManagement WordContentManagement { get; set; }
    }
}
