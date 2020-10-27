// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameEngine.Services
{
    public class DataServiceBase : ServiceBase
    {
        public DataServiceBase(
            ILogger logger,
            IOptions<Options> options,
            IGameRepository repository
        ) : base(logger, options)
        {
            Data = repository;
        }

        protected IGameRepository Data { get; }
    }
}

