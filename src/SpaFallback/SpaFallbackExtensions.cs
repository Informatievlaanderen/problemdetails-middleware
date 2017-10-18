﻿using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hellang.Middleware.SpaFallback
{
    public static class SpaFallbackExtensions
    {
        private const string MarkerKey = "middleware.SpaFallback";

        public static IServiceCollection AddSpaFallback(this IServiceCollection services)
        {
            return services.AddSpaFallback(configure: null);
        }

        public static IServiceCollection AddSpaFallback(this IServiceCollection services, PathString fallbackPath)
        {
            if (!fallbackPath.HasValue)
            {
                throw new ArgumentException("Fallback path must have a value.", nameof(fallbackPath));
            }

            PathString Factory(HttpContext context)
            {
                return fallbackPath;
            }

            return services.AddSpaFallback(options => options.FallbackPathFactory = Factory);
        }

        public static IServiceCollection AddSpaFallback(this IServiceCollection services, Action<SpaFallbackOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure != null)
            {
                services.Configure(configure);
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, StartupFilter>());

            return services;
        }

        public static IApplicationBuilder UseSpaFallback(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Properties[MarkerKey] = true;

            return app.UseMiddleware<SpaFallbackMiddleware>();
        }

        internal static bool ShouldFallback(this HttpContext context, SpaFallbackOptions options)
        {
            if (context.Response.HasStarted)
            {
                return false;
            }

            if (context.Response.StatusCode != StatusCodes.Status404NotFound)
            {
                return false;
            }

            // Fallback only on "hard" 404s, i.e. when the request reached the marker MW.
            if (!context.Items.ContainsKey(MarkerKey))
            {
                return false;
            }

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                return false;
            }

            if (HasFileExtension(context.Request.Path))
            {
                return options.AllowFileExtensions;
            }

            return true;
        }

        internal static bool ShouldThrow(this HttpContext context, SpaFallbackOptions options)
        {
            return context.Response.StatusCode == StatusCodes.Status404NotFound && options.ThrowIfFallbackFails;
        }

        internal static bool HasFileExtension(this PathString path)
        {
            return path.HasValue && Path.HasExtension(path.Value);
        }

        private class StartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    next(app);

                    if (app.Properties.ContainsKey(MarkerKey))
                    {
                        app.UseMiddleware<SpaFallbackMiddleware.Marker>();
                    }
                };
            }
        }
    }
}