// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace GameEngine
{
    public class Options
    {
        public string GamePath { get; set; } = "_data/games";
        public string ProblemPath { get; set; } = "_data/problems";
        public string IsoPath { get; set; } = "_data/_iso";
        public string DownloadUrl { get; set; } = "/invalid";
        public string ChallengePath { get; set; } = "_data/challenges";
        public string ChallengeSpecFileName { get; set; } = "*.y*ml";
        public string ArchivePath { get; set; } = "_data/archive";
        public string ArchiveGamePath { get; set; } = "_data/archive/games";
        public string ArchiveChallengePath { get; set; } = "_data/archive/challenges";
        public string ProblemStateFileName { get; set; } = "_state.json";
        public string FlagWrapper { get; set; } = "flag{(.*)}";
        public string Command { get; set; } = "docker";
        public string CommandArgs { get; set; } = "run --rm -v {0}:/src -v {1}:/dst {2} {3}";
        public int MaxQueueSize { get; set; } = 20;
        public int MaxScriptSeconds { get; set; } = 300;
        public int MaxSessions { get; set; } = 5;
        public int SessionMinutes { get; set; } = 2;
        public string ConsoleUrlTransformKey { get; set; }
        public string ConsoleUrlTransformValue { get; set; }
    }

    public class EngineClients : List<string>
    {

    }

    public class StatsOptions
    {
        public string StorePath { get; set; } = "_data/stats.json";
    }
}

