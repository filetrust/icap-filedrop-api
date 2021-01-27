using System;
using System.ComponentModel.DataAnnotations;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.CloudSdk.Common.Web.Models
{
    public class SasRequest
    {
        [Required]
        public Uri SasUrl { get; set; }

        public ContentManagementFlags ContentManagementFlags { get; set; }
    }
}