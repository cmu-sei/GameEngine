// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;
using Microsoft.Extensions.Logging;

namespace GameEngine.Extensions
{
    public static class LoggingExtensions
    {
        public static void LogStart(this ClientProblem wrapper, ILogger logger)
        {
            logger.LogInformation(EngineEvent.StartProblem,
                "{ProblemId} {ChallengeId} {ChallengeSlug} {TeamId}",
                wrapper.Id,
                wrapper.Problem.ChallengeLink.Id,
                wrapper.Problem.ChallengeLink.Slug,
                wrapper.Problem.Team?.Id
            );
        }

        public static void Log(this ProblemState state, ILogger logger)
        {
            logger.LogInformation(EngineEvent.ProblemStarted,
                "{ProblemId} {ChallengeId} {ChallengeSlug} {TeamId} {Status} {StartedAt} {StoppedAt}",
                state.Id,
                state.ChallengeLink.Id,
                state.ChallengeLink.Slug,
                state.TeamId,
                state.Status,
                state.Start,
                state.End
            );
        }

        public static void Log(this ClientProblemFlag wrapper, ILogger logger)
        {
            logger.LogInformation(EngineEvent.GradeProblem,
                "{ProblemId} {SubmissionId} {Token}",
                wrapper.Id,
                wrapper.ProblemFlag.SubmissionId,
                string.Join(", ", wrapper.ProblemFlag.Tokens).Truncate(40)
            );
        }

        public static void Log(this GradedSubmission flag, ILogger logger)
        {
            logger.LogInformation(EngineEvent.ProblemGraded,
                "{ProblemId} {SubmissionId} {Status} {Timestamp}",
                flag.SubmissionId,
                flag.Status,
                flag.Timestamp
            );

            flag.State.Log(logger);
        }
    }
}

