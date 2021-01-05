// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using System.Collections.Generic;

namespace GameEngine.Data
{
    public interface IGameRepository
    {
        ProblemState GetProblem(string id);

        ChallengeSpec GetChallengeSpec(string name);

        Dictionary<string, ChallengeSpec> GetChallengeSpecs();

        bool ArchiveChallengeSpec(string name);

        bool SaveChallengeSpec(string name, ChallengeSpec challengeSpec);

        bool DeleteChallengeSpec(string name);

        ProblemContext LoadContext(string id);

        ProblemContext LoadContext(Problem problem);

        void SaveContext(ProblemContext context);
    }
}
