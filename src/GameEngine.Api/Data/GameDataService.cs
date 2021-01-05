// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine.Data
{
    public class GameDataService : IHostedService
    {
        public GameDataService(
            GameDataContext dataContext
        ) {
            DataContext = dataContext;
        }

        GameDataContext DataContext { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DataContext.Reload();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
