using System;
using System.ComponentModel.DataAnnotations;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.CloudSdk.Common.Web.Models
{
    public class UrlRequest
    {
        [Required]
        public Uri InputGetUrl { get; set; }

        public ContentManagementFlags ContentManagementFlags { get; set; }
    }
}