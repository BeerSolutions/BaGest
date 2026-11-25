using System;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest.Core;

public class BaGestApplication
{
    public BaGestApplication(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }
}
