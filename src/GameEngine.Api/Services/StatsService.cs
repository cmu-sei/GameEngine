// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace GameEngine.Services
{
    public class StatsService : ServiceBase
    {
        public StatsService(
            ILogger<StatsService> logger,
            IOptions<Options> options
        ) : base(logger, options)
        {
            EngineStats = new Dictionary<string, EngineStat>();
            Queue = new ActionBlock<EngineStat>(s => ProcessStat(s));
            Sessions = new HashSet<SessionTicket>();
        }

        Dictionary<string, EngineStat> EngineStats { get; }
        ActionBlock<EngineStat> Queue { get; }
        ICollection<SessionTicket> Sessions { get; }

        /// <summary>
        /// Add a new EngineStat to the queue for processing
        /// </summary>
        /// <param name="stat"></param>
        public void ReportStat(EngineStat stat)
        {
            Queue.SendAsync(stat);
        }

        /// <summary>
        /// Return wait time in seconds for challenge initialization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ChallengeWaitSeconds(string id)
        {
            return EngineStats.ContainsKey(id)
                ? EngineStats[id].Average
                : 0;
        }

        /// <summary>
        /// Claim a new session
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientId"></param>
        /// <param name="minutes">session duration in minutes</param>
        /// <returns></returns>
        public bool ClaimSession(string id, string clientId, int minutes)
        {
            if (Options.MaxSessions == 0)
                return true;

            if (Sessions.Any(s => s.Id == id))
                return true;

            if (minutes < 1)
                minutes = Options.SessionMinutes;

            lock (Sessions)
            {
                if (Sessions.Count < Options.MaxSessions)
                {
                    Sessions.Add(new SessionTicket
                    {
                        Id = id,
                        ClientId = clientId,
                        StartedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
                    });
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove an existing session
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveSession(string id)
        {
            var session = Sessions.Where(s => s.Id == id).FirstOrDefault();

            if (session != null)
            {
                lock (Sessions)
                {
                    return Sessions.Remove(session);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a forecast of available and reserved sessions
        /// </summary>
        /// <returns></returns>
        public SessionForecast[] SessionForecast()
        {
            List<SessionForecast> result = new List<SessionForecast>();

            for (int i = 0; i < Options.SessionMinutes; i+=30)
            {
                var ts = DateTime.UtcNow.AddMinutes(i);
                int reserved = Sessions.Count(s => ts.CompareTo(s.ExpiresAt) < 0);
                result.Add(new SessionForecast
                {
                    Time = ts,
                    Reserved = reserved,
                    Available = Options.MaxSessions - reserved
                });
            }

            return result.ToArray();
        }

        /// <summary>
        /// Update game engine statistics
        /// </summary>
        /// <param name="s"></param>
        private void ProcessStat(EngineStat s)
        {
            if (EngineStats.ContainsKey(s.Id))
            {
                EngineStats[s.Id].Sum += s.Sum;
                EngineStats[s.Id].Count += s.Count;

                Logger.LogDebug("updated stat {ChallengeId} {Sum} {Count} {Average}", EngineStats[s.Id].Id, EngineStats[s.Id].Sum, EngineStats[s.Id].Count, EngineStats[s.Id].Average);
            }
            else
            {
                EngineStats.Add(s.Id, s);
            }
        }

        /// <summary>
        /// Remove expired sessions
        /// </summary>
        public void PruneSessions()
        {
            var ts = DateTime.UtcNow;
            var items = Sessions.Where(s => ts.CompareTo(s.ExpiresAt) > 0).ToArray();
            lock (Sessions)
            {
                foreach (var item in items)
                    Sessions.Remove(item);
            }
        }

        /// <summary>
        /// Returns Session and EngineStats data
        /// </summary>
        /// <returns></returns>
        public dynamic Backup()
        {
            return new StatsDump {
                Sessions = Sessions.ToArray(),
                Stats = EngineStats.Values.ToArray()
            };
        }

        /// <summary>
        /// Update session and engine statistics
        /// </summary>
        /// <param name="data"></param>
        public void Restore(StatsDump data)
        {
            Sessions.Clear();
            foreach (var session in data.Sessions)
                Sessions.Add(session);

            EngineStats.Clear();
            foreach (var stat in data.Stats)
                EngineStats.Add(stat.Id, stat);
        }
    }
}
