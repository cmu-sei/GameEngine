// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Extensions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameEngine.Data
{
    public class GameRepository : IGameRepository
    {
        public GameRepository(
            IOptions<Options> options,
            ILogger<GameRepository> logger,
            GameDataContext data
        )
        {
            Options = options.Value;
            Logger = logger;
            Data = data;

            serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
        }

        ILogger Logger { get; }
        Options Options { get; }
        GameDataContext Data { get; }
        private ISerializer serializer;

        /// <summary>
        /// Get a team's instance of a problem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ProblemState GetProblem(string id)
        {
            return LoadContext(id)?.ProblemState;
        }

        /// <summary>
        /// Load a ProblemContext
        /// </summary>
        /// <param name="problem"></param>
        /// <returns></returns>
        public ProblemContext LoadContext(Problem problem)
        {
            var context = LoadContext(problem.Id);
            if (context != null)
                return context;

            var spec = Data.ChallengeSpecs[problem.ChallengeLink.Slug].Map<ChallengeSpec>();

            if (spec != null)
            {
                return new ProblemContext
                {
                    Spec = spec,
                    Problem = problem,
                    ProblemState = new ProblemState
                    {
                        Id = problem.Id,
                        ChallengeLink = problem.ChallengeLink,
                        TeamId = problem.Team?.Id
                    },
                    ProblemFolder = Path.Combine(Data.RootPath, Options.ProblemPath, problem.Id),
                    ChallengeFolder = Path.Combine(Data.RootPath, Options.ChallengePath, problem.ChallengeLink.Slug),
                    IsoFolder = Path.Combine(Data.RootPath, Options.IsoPath)
                };
            }
            return null;
        }

        /// <summary>
        /// Save a ProblemContext to a file
        /// </summary>
        /// <param name="context"></param>
        public void SaveContext(ProblemContext context)
        {
            string contextPath = Path.Combine(Data.RootPath, Options.ProblemPath, context.Problem.Id, "_context.json");
            File.WriteAllText(contextPath, JsonConvert.SerializeObject(context, Formatting.Indented));
        }

        /// <summary>
        /// Load a ProblemContext from a file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ProblemContext LoadContext(string id)
        {
            string contextPath = Path.Combine(Data.RootPath, Options.ProblemPath, id, "_context.json");
            if (File.Exists(contextPath))
            {
                // TODO: handle corruption somehow?
                return JsonConvert.DeserializeObject<ProblemContext>(File.ReadAllText(contextPath));
            }
            return null;
        }

        /// <summary>
        /// Move a challenge spec and it's associated files to an archive folder
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ArchiveChallengeSpec(string name)
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddThh-mm-ss-fff");
            string filePath = Path.Combine(Data.RootPath, Options.ChallengePath, name + ".yaml");
            string archivePath = Path.Combine(Data.RootPath, Options.ArchivePath);

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
                Directory.CreateDirectory(Path.Combine(Data.RootPath, Options.ArchiveChallengePath));
                Directory.CreateDirectory(Path.Combine(Data.RootPath, Options.ArchiveGamePath));
            }

            if (File.Exists(filePath))
            {
                File.Move(filePath, Path.Combine(Data.RootPath, Options.ArchiveChallengePath, name + " - " + timeStamp + ".yaml"));
            }

            //TODO: We need to figure out a way to manage files. We can't archive every file each time a change is made to the YAML config file.
            //string folderPath = Path.Combine(Data.RootPath, Options.ChallengePath, name);

            //if (Directory.Exists(folderPath))
            //{
            //    Directory.Move(folderPath, Path.Combine(Data.RootPath, Options.ArchiveChallengePath, name + " - " + timeStamp));
            //}

            return true;
        }

        /// <summary>
        /// Save a ChallengeSpec to a file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="challengeSpec"></param>
        /// <returns></returns>
        public bool SaveChallengeSpec(string name, ChallengeSpec challengeSpec)
        {
            string folderPath = Path.Combine(Data.RootPath, Options.ChallengePath, name);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // TODO: how do we save all of the files that go into the ChallengeSpec?

            string filePath = Path.Combine(Data.RootPath, Options.ChallengePath, name + ".yaml");
            string challengeSpecYaml = serializer.Serialize(challengeSpec);

            File.WriteAllText(filePath, challengeSpecYaml);

            return true;
        }

        /// <summary>
        /// Performs a soft delete by moving files to an archive folder and adding - deleted - to the file/folder name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool DeleteChallengeSpec(string name)
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddThh-mm-ss-fff");
            string filePath = Path.Combine(Data.RootPath, Options.ChallengePath, name + ".yaml");
            string archivePath = Path.Combine(Data.RootPath, Options.ArchivePath);

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
                Directory.CreateDirectory(Path.Combine(Data.RootPath, Options.ArchiveChallengePath));
                Directory.CreateDirectory(Path.Combine(Data.RootPath, Options.ArchiveGamePath));
            }

            if (File.Exists(filePath))
            {
                File.Move(filePath, Path.Combine(Data.RootPath, Options.ArchiveChallengePath, name + " - deleted - " + timeStamp + ".yaml"));
            }

            string folderPath = Path.Combine(Data.RootPath, Options.ChallengePath, name);

            if (Directory.Exists(folderPath))
            {
                Directory.Move(folderPath, Path.Combine(Data.RootPath, Options.ArchiveChallengePath, name + " - deleted - " + timeStamp));
            }

            return true;
        }

        /// <summary>
        /// Get a ChallengeSpec by the slug name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ChallengeSpec GetChallengeSpec(string name)
        {
            if (Data.ChallengeSpecs.ContainsKey(name))
            {
                return Data.ChallengeSpecs[name].Map<ChallengeSpec>();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get all existing challenge specs
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ChallengeSpec> GetChallengeSpecs()
        {
            return Data.ChallengeSpecs;
        }
    }
}
