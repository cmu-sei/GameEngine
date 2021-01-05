// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public class MatchAlphaNumeric : GradingStrategy
    {
        public MatchAlphaNumeric(Options options, ILogger logger, ProblemContext problemContext)
            : base(options, logger, problemContext) { }

        public override bool GradeToken(TokenSpec spec, string token)
        {
            var normalizedCorrectValue = new string(spec.Value.ToCharArray().Where(c => char.IsLetterOrDigit(c) && c < 128).ToArray());
            var normalizedSubmittedValue = new string(token.ToCharArray().Where(c => char.IsLetterOrDigit(c) && c < 128).ToArray());

            return Normalize(normalizedSubmittedValue) == Normalize(normalizedCorrectValue);
        }
    }
}
