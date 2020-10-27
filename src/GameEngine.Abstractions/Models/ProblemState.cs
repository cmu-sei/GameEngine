// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace GameEngine.Models
{
    public class ProblemState
    {
        public string Id { get; set; }
        public ChallengeLink ChallengeLink { get; set; }
        public string TeamId { get; set; }
        public string Text { get; set; }
        public ProblemStatus Status { get; set; }
        public double Percent { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public int EstimatedReadySeconds { get; set; }
        public bool HasGamespace { get; set; }
        public bool GamespaceReady { get; set; }
        public string GamespaceText { get; set; }
        public List<Token> Tokens { get; set; } = new List<Token>();
    }

    public enum ProblemStatus
    {
        None,
        Registered,
        Generating,
        Generated,
        Deploying,
        Deployed,
        Ready,
        Complete,
        Success,
        Failure,
        Error
    }
}
