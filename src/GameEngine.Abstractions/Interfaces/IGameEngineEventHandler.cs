// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;
using System.Threading.Tasks;

namespace GameEngine.Abstractions
{
    public interface IGameEngineEventHandler
    {
        Task Update(ProblemState state);
        Task Update(GradedSubmission submission);
        Task Reload();
    }
}

