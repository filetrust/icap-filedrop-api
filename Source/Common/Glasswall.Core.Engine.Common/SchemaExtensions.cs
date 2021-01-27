using System;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.Core.Engine.Common
{
    public static class SchemaExtensions
    {
        public static contentManagementFlag ToGlasswallConfigurationContentManagementFlag(this ContentManagementFlagAction flag)
        {
            switch (flag)
            {
                case ContentManagementFlagAction.Allow: return contentManagementFlag.allow;
                case ContentManagementFlagAction.Disallow: return contentManagementFlag.disallow;
                case ContentManagementFlagAction.Sanitise: return contentManagementFlag.sanitise;
                default: throw new ArgumentOutOfRangeException(nameof(flag));
            }
        }
    }
}
