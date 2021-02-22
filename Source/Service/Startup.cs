using System;
using System.Diagnostics.CodeAnalysis;
using Glasswall.CloudSdk.AWS.Common.Web;
using Glasswall.CloudSdk.AWS.Rebuild.LivenessCheckers;
using Glasswall.CloudSdk.Common;
using Glasswall.Core.Engine;
using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.GlasswallEngineLibrary;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.FileProcessing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Glasswall.CloudSdk.AWS.Rebuild
{
    [ExcludeFromCodeCoverage]
    public class Startup : AwsCommonStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }
        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            services.AddSingleton<IMetricService, MetricService>();
            services.AddSingleton<IGlasswallVersionService, GlasswallVersionService>();
            services.AddSingleton<IFileTypeDetector, FileTypeDetector>();
            services.AddSingleton<IFileProtector, FileProtector>();
            services.AddSingleton<IFileAnalyser, FileAnalyser>();
            services.AddSingleton<IAdaptor<ContentManagementFlags, string>, GlasswallConfigurationAdaptor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IGlasswallEngineSemaphore, GlasswallEngineSemaphore>();
            services.AddSingleton<IGlasswallEngineSemaphoreExceptionOptions, GlasswallEngineSemaphoreExceptionOptions>();

            services.AddSingleton<IEngineHealthWatcher, EngineHealthWatcher>();
            services.AddSingleton<EngineLivenessChecker>();
            services.AddHealthChecks().AddCheck<EngineLivenessChecker>("engine_timeout_check", null, new[] { "liveness" });

            var p = (int)Environment.OSVersion.Platform;

            if ((p == 4) || (p == 6) || (p == 128))
            {
                services.AddSingleton<IGlasswallFileOperations, LinuxEngineOperations>();
            }
            else
            {
                services.AddSingleton<IGlasswallFileOperations, WindowsEngineOperations>();
            }
        }

        protected override void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("liveness")
            });

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = (check) => true
            });
        }
    }
}