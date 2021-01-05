// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Abstractions.Models;
using GameEngine.Api.Grading;
using GameEngine.Data;
using GameEngine.Exceptions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TopoMojo.Abstractions;

namespace GameEngine.Services
{
    public class GradingService : DataServiceBase, IGradingService
    {
        public GradingService(
            ILogger<GradingService> logger,
            IOptions<Options> options,
            IGameRepository repository,
            ITopoMojoClient mojo
        ) : base(logger, options, repository)
        {
            Mojo = mojo;
        }

        ITopoMojoClient Mojo { get; }

        /// <summary>
        /// Grades a flag submitted for a problem
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public GradedSubmission Grade(ProblemFlag flag)
        {
            ProblemContext context = null;
            GradingResult result = null;
            Logger.LogDebug($"grading {flag.Id}");
            var submissionStatus = SubmissionStatus.Submitted;

            try
            {
                context = Data.LoadContext(flag.Id);

                if (context == null || context.Flag == null)
                    throw new NotFoundException();

                if (context.ProblemState.Status == ProblemStatus.Complete)
                    throw new ProblemCompleteException();

                SaveFlag(context, flag);

                IGradingStrategy strategy = null;

                switch (context.Flag.Type)
                {
                    case FlagType.MatchOutput:
                        strategy = new MatchOutput(Options, Logger, context);
                        break;

                    case FlagType.MatchAll:
                        strategy = new MatchAll(Options, Logger, context);
                        break;

                    case FlagType.MatchAny:
                        strategy = new MatchAny(Options, Logger, context);
                        break;

                    case FlagType.Match:
                        strategy = new Match(Options, Logger, context);
                        break;

                    case FlagType.MatchAlphaNumeric:
                    default:
                        strategy = new MatchAlphaNumeric(Options, Logger, context);
                        break;
                }

                result = strategy.GradeTokens(flag);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Had a problem with grading");
            }
            finally
            {
                if (context != null && result != null)
                {
                    submissionStatus = result.Success ? SubmissionStatus.Passed : SubmissionStatus.Failed;

                    var check = false;

                    // if max submissions <= 0 then we accept
                    // unlimited submissions if unsuccessful
                    if (context.Problem.Settings.MaxSubmissions > 0)
                    {
                        if (context.Spec.IsMultiStage)
                        {
                            // if multi stage we only consider the
                            // last graded token incorrect count
                            check = result.GradedTokens.Last().Status != TokenStatusType.Correct &&
                                flag.Count >= context.Problem.Settings.MaxSubmissions;
                        }
                        else
                        {
                            check = flag.Count >= context.Problem.Settings.MaxSubmissions;
                        }
                    }

                    var isFinal = result.Success || check;

                    if (isFinal)
                    {
                        //_ = Mojo.Stop(flag.Id);

                        context.ProblemState.End = DateTime.UtcNow;
                        //context.ProblemState.GamespaceReady = false;
                        context.ProblemState.Status = result.Success ? ProblemStatus.Success : ProblemStatus.Failure;
                    }

                    context.ProblemState.Percent = result.CorrectPercent;

                    // map TokenSpec detail along with correct answers
                    var problemStateTokens = new List<Token>();

                    int index = 0;
                    foreach (var tokenSpec in context.Flag.Tokens)
                    {
                        var correct = result.GradedTokens.SingleOrDefault(t => t.Index == index && t.Status == TokenStatusType.Correct);

                        problemStateTokens.Add(new Token
                        {
                            Index = index,
                            Label = tokenSpec.Label,
                            Percent = context.Flag.Tokens.Length == 1 ? 100 : tokenSpec.Percent,
                            Status = correct?.Status ?? TokenStatusType.Pending,
                            Timestamp = correct?.Timestamp,
                            Value = correct?.Value
                        });

                        index++;
                    }

                    context.ProblemState.Tokens = problemStateTokens;

                    Data.SaveContext(context);
                }
            }

            Logger.LogDebug($"returning {flag.Id}");

            return new GradedSubmission
            {
                ProblemId = flag.Id,
                SubmissionId = flag.SubmissionId,
                Status = submissionStatus,
                State = context?.ProblemState,
                Timestamp = DateTime.UtcNow,
                Tokens = result?.GradedTokens
            };
        }

        /// <summary>
        /// Save submitted flag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="flag"></param>
        private void SaveFlag(ProblemContext context, ProblemFlag flag)
        {
            string fn = Path.Combine(context.ProblemFolder, flag.SubmissionId + ".json");
            string data = JsonConvert.SerializeObject(flag);
            File.WriteAllText(fn, data);
        }
    }
}
