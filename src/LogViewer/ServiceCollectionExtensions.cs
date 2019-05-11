using LogViewer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogViewer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<LogViewerService>();
            services.Configure<LogViewerServiceOptions>(configuration);
            return services;
        }
    }
}
