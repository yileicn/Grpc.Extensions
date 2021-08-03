using FM.GrpcDashboard.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace FM.GrpcDashboard
{
    public class Startup
    {
        //const string CookieScheme = "GrpcSwagger";

        IHostingEnvironment _env;
        ILogger _logger;
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            _env = env;
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            services.TryAddSingleton<ConsulService, ConsulService>();
            services.AddSingleton<IGrpcReflection, GrpcService>();
            services.AddSingleton<IGrpcReflection, GrpcServiceV2>();
            services.AddSingleton<GrpcServiceProxy>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _logger.LogDebug("FM.GrpcDashboard start...");

            app.Use(async (context, next) =>
            {
                var rq = await FormatRequest(context.Request);
                await next.Invoke();
                var rs = await FormatResponse(context.Response);
                _logger.LogInformation($"{Environment.NewLine}Request: {rq}{Environment.NewLine}Response: {rs}");
            });
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();
            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        private Task<string> FormatRequest(HttpRequest request)
        {
            return Task.FromResult($"{(request.Method + ": " + request.Host.ToString() + request.PathBase.ToString() + request.Path.ToString())}");
        }

        private Task<string> FormatResponse(HttpResponse response)
        {
            return Task.FromResult($"StatusCode:{response.StatusCode}{Environment.NewLine}Headers:{response.Headers.ToJson()}");
        }
    }
}
