// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;

namespace GameEngine.Abstractions
{
    public interface IGradingService
    {
        GradedSubmission Grade(ProblemFlag flag);
    }
}

