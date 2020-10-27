// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public class MatchAny : GradingStrategy
    {
        public MatchAny(Options options, ILogger logger, ProblemContext problemContext)
            : base(options, logger, problemContext) { }

        public override bool GradeToken(TokenSpec spec, string token)
        {            
            return Normalize(spec.Value).Split('|').Contains(Normalize(token));
        }
    }
}

