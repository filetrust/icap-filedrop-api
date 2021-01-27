namespace Glasswall.CloudSdk.Common
{
    public static class Metric
    {
        public const string DetectFileTypeTime = "gw-metric-detect";
        public const string Base64DecodeTime = "gw-metric-decode-base64";
        public const string FileSize = "gw-metric-filesize";
        public const string DownloadTime = "gw-metric-download";
        public const string Version = "gw-version";
        public const string RebuildTime = "gw-metric-rebuild";
        public const string FormFileReadTime = "gw-metric-formfileread";
        public const string UploadSize = "gw-metric-uploadsize";
        public const string UploadTime = "gw-metric-upload";
        public const string UploadEtag = "gw-put-file-etag";
        public const string AnalyseTime = "gw-metric-analyse";
    }
}