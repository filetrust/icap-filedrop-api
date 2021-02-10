using System;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    [Serializable]
    public abstract class ContentManagementFlagsBase
    {
        public Guid Id { get; set; }
        public ContentManagementFlagAction? Metadata { get; set; }
        public ContentManagementFlagAction? InternalHyperlinks { get; set; }
        public ContentManagementFlagAction? ExternalHyperlinks { get; set; }
        public ContentManagementFlagAction? EmbeddedFiles { get; set; }
        public ContentManagementFlagAction? EmbeddedImages { get; set; }
    }
}
