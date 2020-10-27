// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Exceptions;
using GameEngine.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameEngine.Queues
{
    public class Queue<T> : IQueueService<T>
        where T : IClientProblem
    {
        public Queue(
            ILogger logger,
            IOptions<Options> options,
            IServiceProvider sp,
            IHttpClientFactory httpClientFactory
        )
        {
            Services = sp;
            HttpClientFactory = httpClientFactory;

            ActiveIds = new HashSet<string>();
            Options = options.Value;

            ProcessQueue = new ActionBlock<T>(
                async t => await ProcessQueueItem(t),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Options.MaxQueueSize
                }
            );

            WaitQueue = new ActionBlock<T>(
                async t => await ReQueueItem(t)
            );
            Logger = logger;            
        }

        protected Options Options { get; }
        protected ILogger Logger { get; }

        protected IServiceProvider Services { get; }
        protected IHttpClientFactory HttpClientFactory { get; }
        protected ActionBlock<T> ProcessQueue { get; }
        protected ActionBlock<T> WaitQueue { get; }
        protected ICollection<string> ActiveIds { get; }

        public virtual async Task Enqueue(T entity)
        {
            if (entity == null || entity.Id.IsEmpty())
                throw new NotFoundException();

            if (Validate(entity))
                await ProcessQueue.SendAsync(entity);
        }

        protected virtual bool Validate(T entity)
        {
            return true;
        }

        protected async Task ProcessQueueItem(T t)
        {
            Logger.LogDebug($"Out from the ActionBlock comes {t.Id}");

            if (ActiveIds.Contains(t.Id))
            {
                Logger.LogDebug($"Already processing one of {t.Id}; send to WaitQueue");
                await WaitQueue.SendAsync(t);
                return;
            }

            Logger.LogDebug($"Recording as active is {t.Id}");

            ActiveIds.Add(t.Id);

            Logger.LogDebug($"Send to ProcessItem now {t.Id}");

            ProcessItem(t);

            await Task.Delay(0);
        }

        protected virtual void ProcessItem(T t)
        {
            Logger.LogDebug($"THIS is not where you should be, {t.Id}");
        }

        protected virtual async Task ReQueueItem(T t)
        {
            Logger.LogDebug($"Holding pattern. Wait 3, then back to Queue, {t.Id}");
            await Task.Delay(3000);
            await ProcessQueue.SendAsync(t);
        }

        protected virtual async Task FireCallback(T t, object payload, string endpoint)
        {
            Logger.LogDebug($"Sending payload for {t.Id}, to {endpoint}");

            var http = HttpClientFactory.CreateClient(t.Client);
            await http.PostAsJsonAsync(endpoint, payload);
        }
    }
}

