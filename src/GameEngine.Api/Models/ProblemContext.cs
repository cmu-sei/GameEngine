// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using System.Collections.Generic;

namespace GameEngine.Models
{
    public class ProblemContext
    {
        public Problem Problem { get; set; }
        public ProblemState ProblemState { get; set; }
        public ChallengeSpec Spec { get; set; }
        public FlagSpec Flag { get; set; }
        public string ChallengeFolder { get; set; }
        public string ProblemFolder { get; set; }
        public string IsoFolder { get; set; }
        public List<Macro> Macros { get; set; } = new List<Macro>();
        public int FlagIndex { get; set; }
    }
}

