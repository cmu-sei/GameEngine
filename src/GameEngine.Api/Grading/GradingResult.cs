// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public class GradingResult
    {
        ProblemContext ProblemContext { get; }

        public GradingResult(ProblemContext context)
        {
            ProblemContext = context;
        }

        public List<Token> GradedTokens { get; set; } = new List<Token>();

        public bool Success
        {
            get
            {
                return GradedTokens
                    .Where(t => t.Status == TokenStatusType.Correct)
                    .Sum(i => i.Percent) == 100;
            }
        }

        public double CorrectPercent
        {
            get
            {
                return GradedTokens
                    .Where(t => t.Status == TokenStatusType.Correct)
                    .Sum(t => t.Percent);
            }
        }

        public double IncorrectPercent
        {
            get
            {
                return GradedTokens
                    .Where(t => t.Status == TokenStatusType.Incorrect)
                    .Sum(t => t.Percent);
            }
        }
    }
}
