using System;
using System.ComponentModel.DataAnnotations;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.CloudSdk.Common.Web.Models
{
    public class UrlToUrlRequest
    {
        [Required]
        public Uri InputGetUrl { get; set; }

        [Required]
        public Uri OutputPutUrl { get; set; }

        public ContentManagementFlags ContentManagementFlags { get; set; }
    }
}