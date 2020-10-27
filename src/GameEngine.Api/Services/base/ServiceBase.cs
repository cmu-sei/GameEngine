// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameEngine.Services
{
    public class ServiceBase
    {
        public ServiceBase(
            ILogger logger,
            IOptions<Options> options
        ) {
            Logger = logger;
            Options = options.Value;
        }

        protected Options Options { get; }
        protected ILogger Logger { get; }
    }
}

