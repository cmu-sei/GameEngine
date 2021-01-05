// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameEngine.Abstractions
{
    public interface IGameEngineService
    {
        //Task<Game> Load();
        Task<SessionForecast[]> GetForecast();
        Task<bool> ReserveSession(SessionRequest sr);
        Task<bool> CancelSession(string id);
        Task Spawn(Problem problem);
        Task Delete(string id);
        Task Grade(ProblemFlag flag);
        Task<ConsoleSummary> Ticket(string vmId);
        Task ChangeVm(VmAction vmAction);
        Task<ChallengeSpec> ChallengeSpec(string name);
        Task<List<ChallengeSpec>> ChallengeSpecs();
        Task<ChallengeSpec> SaveChallengeSpec(string name, ChallengeSpec challengeSpec);
        Task DeleteChallengeSpec(string name);
    }
}
