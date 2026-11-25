using BaGest.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace BaGest;

public class ConfigureBaGestServer
    : IConfigureOptions<CorsOptions>
    , IConfigureOptions<FormOptions>
    , IConfigureOptions<ForwardedHeadersOptions>
    , IConfigureOptions<IISServerOptions>
{
    public const string CorsPolicy = "AllowAll";
    private readonly BaGestOptions _baGestOptions;

    public ConfigureBaGestServer(IOptions<BaGestOptions> baGestOptions)
    {
        _baGestOptions = baGestOptions.Value;
    }


    public void Configure(CorsOptions options)
    {
        // TODO: Consider disabling this on production builds.
        options.AddPolicy(
            CorsPolicy,
            builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    }

    public void Configure(FormOptions options)
    {
        // Allow packages up to ~8GiB in size
        options.MultipartBodyLengthLimit = (long) _baGestOptions.MaxPackageSizeGiB * int.MaxValue / 2;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

        // Do not restrict to local network/proxy
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }

    public void Configure(IISServerOptions options)
    {
        options.MaxRequestBodySize = (long)_baGestOptions.MaxPackageSizeGiB * int.MaxValue / 2;
    }
}
