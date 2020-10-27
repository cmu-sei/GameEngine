// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataStartupExtensions
    {
        public static IServiceCollection AddDefaultGameDataProvider(this IServiceCollection services)
        {
            services
                .AddSingleton<IGameRepository, GameRepository>()
                .AddSingleton<GameDataContext>()
                .AddHostedService<GameDataService>();

            return services;
        }
    }
}

