using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using BaGest.Aws;
using Microsoft.Extensions.Options;
using Xunit;

namespace BaGest.Aws.Tests;

/// <summary>
/// Runs against a real S3-compatible service (for example MinIO). Set
/// <c>BAGEST_S3_ENDPOINT</c>, <c>BAGEST_S3_ACCESS_KEY</c> and <c>BAGEST_S3_SECRET_KEY</c> to enable.
/// </summary>
public sealed class RequiresS3ServiceFactAttribute : FactAttribute
{
    public const string EndpointVariable = "BAGEST_S3_ENDPOINT";
    public const string AccessKeyVariable = "BAGEST_S3_ACCESS_KEY";
    public const string SecretKeyVariable = "BAGEST_S3_SECRET_KEY";

    public RequiresS3ServiceFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EndpointVariable)))
        {
            Skip = $"Set {EndpointVariable}, {AccessKeyVariable} and {SecretKeyVariable} (e.g. for MinIO) to run.";
        }
    }
}

public sealed class RequiresS3ServiceTheoryAttribute : TheoryAttribute
{
    public RequiresS3ServiceTheoryAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RequiresS3ServiceFactAttribute.EndpointVariable)))
        {
            Skip = $"Set {RequiresS3ServiceFactAttribute.EndpointVariable}, {RequiresS3ServiceFactAttribute.AccessKeyVariable} and {RequiresS3ServiceFactAttribute.SecretKeyVariable} (e.g. for MinIO) to run.";
        }
    }
}

public class S3StorageTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ForcePathStyleIsAppliedToTheClientConfigWhenAnEndpointIsSet(bool forcePathStyle)
    {
        var options = new S3StorageOptions
        {
            Endpoint = new Uri("http://localhost:9000"),
            ForcePathStyle = forcePathStyle,
        };

        var config = AwsApplicationExtensions.CreateS3Config(options);

        Assert.Equal(forcePathStyle, config.ForcePathStyle);
        Assert.Equal("http://localhost:9000/", config.ServiceURL);
    }

    [Fact]
    public void RegionIsUsedForSigningWhenAnEndpointIsAlsoSet()
    {
        var options = new S3StorageOptions
        {
            Endpoint = new Uri("http://localhost:9000"),
            Region = "eu-west-1",
        };

        var config = AwsApplicationExtensions.CreateS3Config(options);

        Assert.Equal("eu-west-1", config.AuthenticationRegion);
        Assert.Equal("http://localhost:9000/", config.ServiceURL);
    }

    [Fact]
    public void AuthenticationRegionIsNotSetWithoutARegion()
    {
        var options = new S3StorageOptions { Endpoint = new Uri("http://localhost:9000") };

        Assert.Null(AwsApplicationExtensions.CreateS3Config(options).AuthenticationRegion);
    }

    [Fact]
    public void EndpointAndRegionTogetherAreValid()
    {
        var options = new S3StorageOptions
        {
            Endpoint = new Uri("http://localhost:9000"),
            Region = "eu-west-1",
            Bucket = "bucket",
        };

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);

        Assert.True(valid, string.Join("; ", results.Select(r => r.ErrorMessage)));
    }

    [Fact]
    public void ChunkEncodingIsEnabledByDefault()
    {
        Assert.True(new S3StorageOptions().UseChunkEncoding);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ForcePathStyleIsAppliedToTheClientConfigWhenARegionIsSet(bool forcePathStyle)
    {
        var options = new S3StorageOptions { Region = "eu-west-1", ForcePathStyle = forcePathStyle };

        var config = AwsApplicationExtensions.CreateS3Config(options);

        Assert.Equal(forcePathStyle, config.ForcePathStyle);
    }

    [RequiresS3ServiceTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PutAndGetRoundTripWithForcePathStyle(bool useChunkEncoding)
    {
        var endpoint = Environment.GetEnvironmentVariable(RequiresS3ServiceFactAttribute.EndpointVariable);
        var credentials = new BasicAWSCredentials(
            Environment.GetEnvironmentVariable(RequiresS3ServiceFactAttribute.AccessKeyVariable),
            Environment.GetEnvironmentVariable(RequiresS3ServiceFactAttribute.SecretKeyVariable));

        var options = new S3StorageOptions
        {
            Endpoint = new Uri(endpoint),
            ForcePathStyle = true,
            Bucket = "bagest-" + Guid.NewGuid().ToString("N"),
            Prefix = "packages",
            UseChunkEncoding = useChunkEncoding,
        };

        using var client = new AmazonS3Client(credentials, AwsApplicationExtensions.CreateS3Config(options));
        await client.PutBucketAsync(options.Bucket);

        try
        {
            var storage = new S3StorageService(new SnapshotOptions(options), client);

            using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            var result = await storage.PutAsync("a\\b.txt", content, "text/plain", CancellationToken.None);
            Assert.Equal(BaGest.Core.StoragePutResult.Success, result);

            using var downloaded = await storage.GetAsync("a/b.txt", CancellationToken.None);
            using var reader = new StreamReader(downloaded);
            Assert.Equal("hello", await reader.ReadToEndAsync());
        }
        finally
        {
            var listing = await client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request { BucketName = options.Bucket });
            foreach (var o in listing.S3Objects)
            {
                await client.DeleteObjectAsync(options.Bucket, o.Key);
            }

            await client.DeleteBucketAsync(options.Bucket);
        }
    }

    private sealed class SnapshotOptions(S3StorageOptions value) : IOptionsSnapshot<S3StorageOptions>
    {
        public S3StorageOptions Value => value;

        public S3StorageOptions Get(string name) => value;
    }
}
