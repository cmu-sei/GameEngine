// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;
using GameEngine.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine.HostedServices
{
    public class StatsHostedService : IHostedService, IDisposable
    {
        public StatsHostedService(
            ILogger<StatsHostedService> logger,
            StatsService statsService,
            StatsOptions statsOptions,
            IHostEnvironment env
        )
        {
            Logger = logger;
            Stats = statsService;
            BackupFile = Path.Combine(env.ContentRootPath, statsOptions.StorePath);

            string path = Path.GetDirectoryName(BackupFile);

            Directory.CreateDirectory(
                Path.GetDirectoryName(BackupFile)
            );
        }

        string BackupFile { get; }
        ILogger Logger { get; }
        StatsService Stats { get; }
        Timer Timer { get; set; }

        /// <summary>
        /// Update session and engine statistics from file
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(BackupFile))
                Stats.Restore(JsonConvert.DeserializeObject<StatsDump>(File.ReadAllText(BackupFile)));

            Timer = new Timer(RunMaintenance, null, 2000, 60000);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Clean up sessions and save data to file
        /// </summary>
        /// <param name="state"></param>
        private void RunMaintenance(object state)
        {
            Stats.PruneSessions();

            File.WriteAllText(BackupFile, JsonConvert.SerializeObject(Stats.Backup()));
        }

        /// <summary>
        /// Run cleanup tasks
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            RunMaintenance(null);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Dispose of the Timer
        /// </summary>
        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
