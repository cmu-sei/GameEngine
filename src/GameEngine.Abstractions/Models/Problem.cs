// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Models
{
    public class Problem
    {
        public string Id { get; set; }
        public ChallengeLink ChallengeLink { get; set; } = new ChallengeLink();
        public PlayerTeam Team { get; set; }
        public int? FlagIndex { get; set; }
        public GameSettings Settings { get; set; } = new GameSettings();
        public string IsolationId { get; set; }
    }

    public class GameSettings
    {
        public int MaxSubmissions { get; set; }
        public bool IsPractice { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string BoardId { get; set; }
        public string BoardName { get; set; }
    }

    public class ChallengeLink
    {
        public string Id { get; set; }

        public string Slug { get; set; }
    }
}

