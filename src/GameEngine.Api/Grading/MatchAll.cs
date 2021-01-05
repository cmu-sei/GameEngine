// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public class MatchAll : GradingStrategy
    {
        public MatchAll(Options options, ILogger logger, ProblemContext problemContext)
            : base(options, logger, problemContext) { }


        public override bool GradeToken(TokenSpec spec, string token)
        {
            bool match = true;

            var value = Normalize(token);

            Normalize(spec.Value).Split('|').ToList().ForEach(v =>
            {
                match &= value.Contains(v);
            });

            return match;
        }
    }
}
