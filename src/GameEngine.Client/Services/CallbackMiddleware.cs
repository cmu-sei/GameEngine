// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Client
{
    public class CallbackMiddleware
    {
        RequestDelegate _next;
        ILogger _logger;
        Options _options;

        public CallbackMiddleware(
            RequestDelegate next,
            ILogger<CallbackMiddleware> logger,
            Options options
        )
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IGameEngineEventHandler handler)
        {
            if (context.Request.Path.Value.StartsWith(_options.CallbackEnpoint))
            {
                string apiKey = context.Request.Headers["x-api-key"].FirstOrDefault();
                if (apiKey != _options.GameEngineKey)
                {
                    context.Response.StatusCode = 401;
                    context.Response.Headers.Add("WWW-Authentication", "x-api-key");
                    return;
                }

                using (StreamReader sr = new StreamReader(context.Request.Body))
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    try
                    {
                        string ev = context.Request.Path.Value.Split('/').Last();
                        switch (ev)
                        {
                            case "updated":
                            var problemState = serializer.Deserialize<ProblemState>(jr);
                            await handler.Update(problemState);
                            break;

                            case "graded":
                            var gradedFlag = serializer.Deserialize<GradedSubmission>(jr);
                            await handler.Update(gradedFlag);
                            break;

                            case "reload":
                            await handler.Reload();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erroring processing GameEngine callback message.");
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}

