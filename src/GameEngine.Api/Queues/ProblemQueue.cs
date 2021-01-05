// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Extensions;
using GameEngine.Models;
using GameEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameEngine.Queues
{
    public class ProblemQueue : Queue<ClientProblem>
    {
        public ProblemQueue(
            ILogger<ProblemQueue> logger,
            IOptions<Options> options,
            IServiceProvider sp,
            IProblemService svc,
            StatsService statsService,
            IHttpClientFactory httpClientFactory
        ) : base(logger, options, sp, httpClientFactory)
        {
            Service = svc;
            Stats = statsService;
        }

        private IProblemService Service { get; }
        private StatsService Stats { get; }

        protected override void ProcessItem(ClientProblem wrapper)
        {
            var state = new ProblemState();

            try
            {
                wrapper.LogStart(Logger);

                state = Service.Spawn(wrapper.Problem);

                Task.Delay(2000).Wait();

                _ = FireCallback(wrapper, state, "updated");

                state.Log(Logger);
            }
            finally
            {
                ActiveIds.Remove(wrapper.Id);
            }
        }

        protected override bool Validate(ClientProblem wrapper)
        {
            int eta = Stats.ChallengeWaitSeconds(wrapper.Problem.ChallengeLink.Id);

            var state = new ProblemState
            {
                Id = wrapper.Id,
                ChallengeLink = wrapper.Problem.ChallengeLink,
                TeamId = wrapper.Problem.Team?.Id,
                Status = ProblemStatus.Registered,
                EstimatedReadySeconds = eta,
                Text = (eta > 0)
                    ? $"Average wait time is {eta} seconds"
                    : "Challenge is initializing..."
            };

            _ = FireCallback(wrapper, state, "updated");

            return true;
        }

        protected override Task ReQueueItem(ClientProblem wrapper)
        {
            Logger.LogDebug($"dropping dupe problem start {wrapper.Id}");
            return Task.FromResult(0);
        }
    }
}
