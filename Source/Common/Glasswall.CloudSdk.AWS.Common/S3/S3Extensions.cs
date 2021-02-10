using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Glasswall.CloudSdk.AWS.Common.S3
{
    public static class S3Extensions
    {
        public static async Task<byte[]> DownloadS3Object(this IAmazonS3 s3Client, string bucket, string key)
        {
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));
            if (string.IsNullOrWhiteSpace(bucket)) throw new ArgumentNullException(nameof(bucket));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            using (var response = await s3Client.GetObjectAsync(bucket, key))
            {
                using (var s3Contents = response.ResponseStream)
                {
                    using (var ms = new MemoryStream())
                    {
                        s3Contents.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

        public static async Task PutByteArray(this IAmazonS3 s3Client, byte[] bytes, string bucket, string key)
        {
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));
            if (string.IsNullOrWhiteSpace(bucket)) throw new ArgumentNullException(nameof(bucket));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            using (var protectedFileStream = new MemoryStream(bytes))
            {
                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    Key = key,
                    BucketName = bucket,
                    InputStream = protectedFileStream
                });
            }
        }
    }
}