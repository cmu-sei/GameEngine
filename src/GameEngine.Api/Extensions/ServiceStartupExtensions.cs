// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Api.Abstractions;
using GameEngine.Api.Services;
using GameEngine.HostedServices;
using GameEngine.Models;
using GameEngine.Queues;
using GameEngine.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceStartupExtensions
    {
        public static IServiceCollection AddDefaultGameServices(
            this IServiceCollection services
        )
        {
            services
                .AddSingleton<IProblemService, ProblemService>()
                .AddSingleton<IGradingService, GradingService>()
                .AddSingleton<IQueueService<ClientProblem>, ProblemQueue>()
                .AddSingleton<IQueueService<ClientProblemFlag>, GradingQueue>()
                .AddSingleton<StatsService>()
                .AddSingleton<IChallengeService, ChallengeService>()
                .AddHostedService<StatsHostedService>();

            return services;
        }
    }
}
