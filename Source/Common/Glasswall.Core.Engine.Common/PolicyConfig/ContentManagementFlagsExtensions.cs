using System;
using System.Linq;

namespace Glasswall.Core.Engine.Common.PolicyConfig
{
    public static class ContentManagementFlagsExtensions
    {
        /// <summary>
        /// Validates the input content management and sets the unspecified fields to default
        /// </summary>
        /// <param name="contentManagementFlags"></param>
        /// <returns></returns>
        public static ContentManagementFlags ValidatedOrDefault(this ContentManagementFlags contentManagementFlags)
        {
            if (contentManagementFlags == null)
                return Policy.DefaultContentManagementFlags;

            foreach (var property in typeof(ContentManagementFlags).GetProperties())
            {
                if (!property.PropertyType.IsSubclassOf(typeof(ContentManagementFlagsBase))) continue;

                var inputFlagSection = property.GetValue(contentManagementFlags);
                var defaultFlagSection = property.GetValue(Policy.DefaultContentManagementFlags);

                if (inputFlagSection == null)
                {
                    property.SetValue(contentManagementFlags, defaultFlagSection);
                }
                else
                {
                    foreach (var flagProps in
                        inputFlagSection.GetType()
                            .GetProperties()
                            .Where(s => s.PropertyType == typeof(ContentManagementFlagAction?)))
                    {
                        var inputFlag = flagProps.GetValue(inputFlagSection);
                        var defaultFlag = flagProps.GetValue(defaultFlagSection);

                        if (inputFlag == null)
                            flagProps.SetValue(inputFlagSection, defaultFlag);
                    }
                }
            }

            return contentManagementFlags;
        }
    }
}
