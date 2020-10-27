// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Api.Abstractions
{
    public interface IChallengeService
    {
        ChallengeSpec GetChallengeSpec(string name);

        List<ChallengeSpec> GetChallengeSpecs();

        ChallengeSpec SaveChallengeSpec(string name, ChallengeSpec challengeSpec);

        bool DeleteChallengeSpec(string name);
    }
}

