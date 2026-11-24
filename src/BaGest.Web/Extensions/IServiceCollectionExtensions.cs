using System;
using System.Text.Json.Serialization;
using BaGest.Authentication;
using BaGest.Core;
using BaGest.Web;
using BaGest.Web.Authentication;
using BaGest.Web.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBaGestWebApplication(
        this IServiceCollection services,
        Action<BaGestApplication> configureAction)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true)
            .AddControllers()
            .AddApplicationPart(typeof(PackageContentController).Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddRazorPages();

        services.AddHttpContextAccessor();
        services.AddTransient<IUrlGenerator, BaGestUrlGenerator>();

        services.AddSingleton(ApplicationVersionHelper.GetVersion());

        var app = services.AddBaGestApplication(configureAction);

        return services;
    }
}
