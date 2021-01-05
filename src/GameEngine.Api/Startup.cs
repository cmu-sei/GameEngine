// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ClientKeys = new Dictionary<string, string>();
            var clients = Configuration.GetSection("ClientKeys").Get<string[]>() ?? new string[]{};
            foreach (string client in clients)
            {
                var parts = client.Split(";");
                ClientKeys.Add(parts[0], parts[1]);
            }

            StatsOptions = Configuration.GetSection("Stats").Get<StatsOptions>() ?? new StatsOptions();
        }

        Dictionary<string, string> ClientKeys { get; set; }

        public IConfiguration Configuration { get; }

        StatsOptions StatsOptions { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                });

            services
                .AddOptions()
                    .Configure<Options>(Configuration.GetSection("Engine"))
                    .Configure<EngineClients>(Configuration.GetSection("ClientKeys"))

                .AddSingleton<StatsOptions>(sp => StatsOptions)
                .AddDefaultGameDataProvider()
                .AddDefaultGameServices();

            foreach (string key in ClientKeys.Keys)
            {
                services.AddHttpClient(key.Untagged(), c => {
                    c.BaseAddress = new Uri(ClientKeys[key]);
                    c.DefaultRequestHeaders.Add("x-api-key", key);
                }).AddPolicyHandler(
                    HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                );
            }

            services.AddTopoMojoClient(() => Configuration.GetSection("TopoMojo").Get<TopoMojo.Client.Options>());
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.StartsWith("/api/"))
                {
                    string key = context.Request.Headers["x-api-key"];
                    if (key.IsEmpty() || !ClientKeys.ContainsKey(key))
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }
                    context.Request.Headers.Add("x-api-client", key.Untagged());
                    context.Request.Headers.Add("x-api-callback", ClientKeys[key]);
                }

                await next();
            });

            app.UseRouting();

            app.UseEndpoints(ep =>
            {
                ep.MapDefaultControllerRoute();
            });
        }
    }
}
