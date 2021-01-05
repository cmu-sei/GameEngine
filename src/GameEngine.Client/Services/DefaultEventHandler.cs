// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GameEngine.Client
{
    public class DefaultEventHandler : IGameEngineEventHandler
    {
        public DefaultEventHandler(
            ILogger<DefaultEventHandler> logger
        )
        {
            Logger = logger;
        }

        ILogger Logger { get; }

        public async Task Update(ProblemState state)
        {
            Logger.LogInformation(JsonConvert.SerializeObject(state));
            await Task.Delay(0);
        }

        public async Task Update(GradedSubmission submission)
        {
            Logger.LogInformation(JsonConvert.SerializeObject(submission));
            await Task.Delay(0);
        }

        public async Task Reload()
        {
            Logger.LogInformation("Received reload event from GameEngine");
            await Task.Delay(0);
        }
    }
}
