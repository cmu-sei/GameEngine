// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameEngine.Client
{
    public class ProxyMock : IGameEngineService
    {
        public ProxyMock(
            IMemoryCache cache,
            IHostingEnvironment env,
            Options options,
            IGameEngineEventHandler handler,
            //IGameDataCache dataCache,
            ILogger<ProxyMock> logger
        )
        {
            Cache = cache;
            Env = env;
            Options = options;
            Handler = handler;
            Logger = logger;
            //DataCache = dataCache;
        }

        Options Options { get; }
        //IGameDataCache DataCache { get; }
        IMemoryCache Cache { get; }
        IHostingEnvironment Env { get; }
        IGameEngineEventHandler Handler { get; }
        ILogger Logger { get; }

        public Task Grade(ProblemFlag flag)
        {
            bool passed = false;

            if (!Cache.TryGetValue(Key(flag.Id), out ProblemState state))
                throw new Exception("Problem not found.");

            if (state.End != null && state.End != DateTime.MinValue)
                throw new Exception("Problem already complete.");

            Logger.LogDebug("received flag");

            var graded = new GradedSubmission
            {
                SubmissionId = flag.SubmissionId,
                ProblemId = flag.Id,
                Timestamp = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted,
                State = state
            };

            Task.WaitAll(Handler.Update(graded));

            Logger.LogDebug("grading flag");

            if (Cache.TryGetValue(FlagKey(flag.Id), out string target))
            {
                passed = flag.Tokens[0] == target;
            }

            if (passed || flag.Count >= 3)
            {
                state.End = DateTime.UtcNow;
                state.Status = passed ? ProblemStatus.Success : ProblemStatus.Failure;
                state.Percent = passed ? 10 : 0;
                state.GamespaceReady = false;
            }

            graded.Status = passed ? SubmissionStatus.Passed : SubmissionStatus.Failed;

            Task.Delay(new Random().Next(2000, 6000)).Wait();

            Logger.LogDebug("graded flag");

            Task.WaitAll(Handler.Update(graded));

            return Task.CompletedTask;
        }

        public Task Spawn(Problem problem)
        {
            if (Cache.TryGetValue(Key(problem.Id), out ProblemState existing))
            {
                existing.GamespaceReady = true;
                existing.Status = ProblemStatus.Ready;
                Task.WaitAll(Handler.Update(existing));

                return Task.FromResult(existing);
            }

            int i = new Random().Next(4,12);

            var state = new ProblemState
            {
                Id = problem.Id,
                ChallengeLink = problem.ChallengeLink,
                TeamId = problem.Team.Id,
                Status = ProblemStatus.Registered,
                Start = DateTime.UtcNow,
                EstimatedReadySeconds = i,
                Text = $"Preparing...estimated wait time: {i} seconds"
            };

            Task.WaitAll(Handler.Update(state));

            Logger.LogDebug("generating flag");

            string flag = new Random().Next().ToString("x");
            Cache.Set(FlagKey(problem.Id), flag);

            state.Text = $"> Download Resources: [PDF Instructions](#) | [ISO File](#)\n\n> Gamespace Resources: [windows10](/console/#) | [kali](/console/#)\n\n(flag: `{flag}`, for testing)\n\n## Demo Instructions\n\nIn this challenge you will find the flag using tools and procedures you are most likely familiar with as a cybersecurity operator.\n\n#### Concept\n\nContinue to work through this...";
            Cache.Set(Key(problem.Id), state);

            Logger.LogDebug("updating state");

            state.Start = DateTime.UtcNow;
            state.Status = ProblemStatus.Ready;
            state.EstimatedReadySeconds = 0;
            state.HasGamespace = true;
            state.GamespaceReady = true;

            Logger.LogDebug("mock delay");

            Task.Delay(i * 1000).Wait();
            Task.WaitAll(Handler.Update(state));

            Logger.LogDebug("done");

            return Task.CompletedTask;
        }

        private string Key(string id)
        {
            return $"mockEngine-state#{id}";
        }

        private string FlagKey(string id)
        {
            return $"mockEngine-flag#{id}";
        }

        //public Task<Game> Load()
        //{
        //    string gameId = Options.GameId;
        //    if (!gameId.EndsWith(".json")) gameId += ".json";

        //    string fn = Path.Combine(Env.ContentRootPath, Options.LocalPath, gameId);

        //    if (!File.Exists(fn))
        //        throw new Exception($"Missing file {fn}");

        //    var game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(fn));

        //    DataCache.Load(game);

        //    return Task.FromResult(game);
        //}

        public Task<ConsoleSummary> Ticket(string vmId)
        {
            return Task.FromResult(new ConsoleSummary());
        }

        public Task Delete(string id)
        {
            if (Cache.TryGetValue(Key(id), out ProblemState state))
            {
                state.GamespaceReady = false;
            }

            return Task.CompletedTask;
        }

        public Task<SessionForecast[]> GetForecast()
        {
            return Task.FromResult(new SessionForecast[] {
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(0), Reserved = 20, Available = 0 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(30), Reserved = 20, Available = 0 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(60), Reserved = 18, Available = 2 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(90), Reserved = 15, Available = 5 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(120), Reserved = 11, Available = 9 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(150), Reserved = 6, Available = 14 },
                new SessionForecast { Time = DateTime.UtcNow.AddMinutes(180), Reserved = 1, Available = 19 }
            });
        }

        public Task<bool> ReserveSession(SessionRequest sr)
        {
            if (sr.SessionId.ToLower() == "testfail")
                throw new Exception("All 20 sessions are in use. Please try again later.");

            return Task.FromResult(true);
        }

        public Task<bool> CancelSession(string id)
        {
            if (id.ToLower() == "testfail")
                throw new Exception($"Session {id} could not be canceled. Please try again later.");

            return Task.FromResult(true);
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            await Task.Delay(2000);
        }

        public Task<ChallengeSpec> ChallengeSpec(string name)
        {
            if (Cache.TryGetValue<ChallengeSpec>(Key(name), out ChallengeSpec challengeSpec))
            {
                return Task.FromResult(challengeSpec);
            }
            else
            {
                return null;
            }
        }

        public Task<List<ChallengeSpec>> ChallengeSpecs()
        {
            //TODO: it would be better if we could retrieve the items that were added by save method.
            //There is not a way to enumerate keys on IMemoryCache
            List<ChallengeSpec> challengeSpecs = new List<ChallengeSpec>();
            ChallengeSpec challengeSpec = GetChallengeSpec();
            challengeSpecs.Add(challengeSpec);

            return Task.FromResult(challengeSpecs);
        }

        public Task<ChallengeSpec> SaveChallengeSpec(string name, ChallengeSpec challengeSpec)
        {
            if(!string.IsNullOrEmpty(name) && challengeSpec != null)
            {
                challengeSpec.Slug = name;
                Cache.Set(name, challengeSpec);

                return Task.FromResult(challengeSpec);
            }
            else
            {
                throw new ArgumentException("The ChallengeSpec save operation failed.");
            }
        }

        public Task DeleteChallengeSpec(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Cache.Remove(name);
                return Task.CompletedTask;
            }
            else
            {
                throw new ArgumentException("The ChallengeSpec delete operation failed.");
            }
        }

        private ChallengeSpec GetChallengeSpec()
        {
            ChallengeSpec challengeSpec = new ChallengeSpec();
            challengeSpec.Title = "The Test Game";
            challengeSpec.Tags = "Sift, Forensics";
            challengeSpec.Document = "AI500 - T1.pdf";
            challengeSpec.Description = " | Use Sift workstation to analyze evidence drives **Analysis using the virtual gamespace required **";
            challengeSpec.Workspace = new WorkspaceSpec
            {
                Id = 104,
                CustomizeTemplates = true,
                Vms = new VmSpec[] { new VmSpec
                {
                    Name = "Windows10",
                    Replicas = 3
                } }
            };
            challengeSpec.Flags = new FlagSpec[] {
                new FlagSpec
                {
                    Tokens = new TokenSpec[]{ new TokenSpec { Value = "ncrypt10n_ch4ll3ng1ng" } }
                },
                new FlagSpec
                {
                    Tokens = new TokenSpec[]{ new TokenSpec { Value = "ncrypt10n_h3lps" } },
                    GenerateCommand = "sed -i \"s/evidence1.vmdk/evidence2.vmdk/\" /dst/_templates.json",
                    GenerateImage = "bash"
                },
                new FlagSpec
                {
                    Tokens = new TokenSpec[]{ new TokenSpec { Value = "b1tl0ck3r" } },
                    GenerateCommand = "sed -i \"s/evidence1.vmdk/evidence3.vmdk/\" /dst/_templates.json",
                    GenerateImage = "bash"
                }
            };

            return challengeSpec;
        }
    }
}

