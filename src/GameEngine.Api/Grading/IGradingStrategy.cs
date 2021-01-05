// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Logging;

namespace GameEngine.Api.Grading
{
    public interface IGradingStrategy
    {
        ILogger Logger { get; }
        ProblemContext ProblemContext { get; }

        bool Unwrap { get; }

        GradingResult GradeTokens(ProblemFlag problemFlag);

        bool GradeToken(TokenSpec spec, string token);
    }
}
