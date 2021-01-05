// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameEngine.Data
{
    public class GameDataContext
    {
        public GameDataContext (
            IOptions<Options> options,
            ILogger<GameDataContext> logger,
            IHostEnvironment env
        ) {
            Options = options.Value;
            Logger = logger;
            Env = env;

            yd = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();

            Directory.CreateDirectory(Options.ProblemPath);
            Directory.CreateDirectory(Options.ChallengePath);
            Directory.CreateDirectory(Options.ArchiveChallengePath);
            Directory.CreateDirectory(Options.GamePath);
            Directory.CreateDirectory(Options.ArchiveGamePath);
        }

        ILogger Logger { get; }
        Options Options { get; }
        IHostEnvironment Env { get; }

        private IDeserializer yd;

        public string RootPath { get { return Env.ContentRootPath; }}
        public Dictionary<string, string> ContextMap { get; } = new Dictionary<string, string>();
        public Dictionary<string, ChallengeSpec> ChallengeSpecs { get; private set; } = new Dictionary<string, ChallengeSpec>();
        public IEnumerable<ChallengeSpec> Specs { get { return ChallengeSpecs.Values; } }

        /// <summary>
        /// Reload game and challenge data from source
        /// </summary>
        public void Reload()
        {
            var specs = new Dictionary<string, ChallengeSpec>();
            foreach (string file in Directory.GetFiles(Options.ChallengePath, Options.ChallengeSpecFileName, SearchOption.AllDirectories))
            {
                try
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    var spec = yd.Deserialize<ChallengeSpec>(
                        File.ReadAllText(file)
                    );
                    specs.Add(key, spec);
                } catch {
                    Logger.LogError($"Failed to load challenge {file}");
                }
            }

            ChallengeSpecs = specs;
        }
    }
}
