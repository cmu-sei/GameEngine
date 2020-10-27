// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Client;
using Polly;
using Polly.Extensions.Http;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddGameEngineClient(
            this IServiceCollection services,
            Func<GameEngine.Client.Options> config
        )
        {
            var options = config.Invoke();

            services.AddSingleton(sp => options);
            //services.AddSingleton<GameDataContext>();
            //services.AddSingleton<IGameDataCache, GameDataCache>();
            services.AddScoped<IGameEngineEventHandler, DefaultEventHandler>();

            if (Uri.TryCreate(options.GameEngineUrl, UriKind.Absolute, out Uri uri))
            {
                services.AddScoped<IGameEngineService, Proxy>();

                services.AddHttpClient<IGameEngineService, Proxy>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Add("x-api-key", options.GameEngineKey);
                })
                .AddPolicyHandler(
                    HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(options.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                );
            }
            else
            {
                services.AddScoped<IGameEngineService, ProxyMock>();
            }

            return services;
        }
    }
}

