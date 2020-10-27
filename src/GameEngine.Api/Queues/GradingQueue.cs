// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Extensions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameEngine.Queues
{
    public class GradingQueue : Queue<ClientProblemFlag>
    {
        public GradingQueue(
            IServiceProvider sp,
            ILogger<GradingQueue> logger,
            IOptions<Options> options,
            IGradingService svc,
            IHttpClientFactory http
        ) : base(logger, options, sp, http)
        {
            Service = svc;
        }

        private IGradingService Service { get; }

        /// <summary>
        /// Process a flag for a problem submission
        /// </summary>
        /// <param name="wrapper"></param>
        protected override void ProcessItem(ClientProblemFlag wrapper)
        {
            try
            {
                wrapper.Log(Logger);

                var result = Service.Grade(wrapper.ProblemFlag);

                Task.Delay(2000).Wait();

                _ = FireCallback(wrapper, result, "graded");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to process grade for {wrapper.Id}");
            }
            finally
            {
                ActiveIds.Remove(wrapper.Id);

                Logger.LogDebug($"Removed from active list, {wrapper.Id}");

            }
        }
    }
}

