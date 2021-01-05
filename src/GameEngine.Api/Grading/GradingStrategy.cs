// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Api.Extensions;
using GameEngine.Exceptions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public abstract class GradingStrategy : IGradingStrategy
    {
        public ILogger Logger { get; }

        public Options Options { get; }

        public ProblemContext ProblemContext { get; }

        public virtual bool Unwrap => true;

        protected GradingStrategy(Options options, ILogger logger, ProblemContext problemContext)
        {
            Options = options;
            Logger = logger;
            ProblemContext = problemContext;
        }

        public abstract bool GradeToken(TokenSpec spec, string token);

        public GradingResult GradeTokens(ProblemFlag problemFlag)
        {
            if (ProblemContext.Flag.Tokens.Length == 0)
                throw new ProblemGradingException("No tokens defined. Problem Id: " + ProblemContext.Problem.Id);

            if (problemFlag.Tokens.Length != ProblemContext.Flag.Tokens.Length)
                throw new ProblemGradingException("The number of tokens submitted is not the same as the number of tokens for the challenge. Submitted token: " +
                    string.Join(" ", problemFlag.Tokens) + " - Problem Id: " + ProblemContext.Problem.Id);

            var result = new GradingResult(ProblemContext);

            var gradedTokens = new List<Token>();

            var timestamp = DateTime.UtcNow;

            var tokens = Unwrap
                ? problemFlag.Tokens.Select(t => t.Unwrap(Options))
                : problemFlag.Tokens;

            int index = 0;

            foreach (string token in tokens)
            {
                var existing = GetExistingToken(token, index);

                if (existing == null)
                {
                    var spec = ProblemContext.Flag.Tokens[index];

                    if (spec == null)
                        throw new TokenSpecNotFoundException();

                    var match = GradeToken(spec, token);

                    var percent = match
                        ? (ProblemContext.Flag.Tokens.Length == 1 ? 100 : spec.Percent)
                        : 0;

                    gradedTokens.Add(
                        new Token
                        {
                            Value = token,
                            Percent = percent,
                            Status = match ? TokenStatusType.Correct : TokenStatusType.Incorrect,
                            Timestamp = timestamp,
                            Index = index
                        });

                    if (ProblemContext.Spec.IsMultiStage)
                    {
                        break;
                    }
                }
                else
                {
                    gradedTokens.Add(existing);
                }

                index++;
            }

            result.GradedTokens = gradedTokens.OrderBy(t => t.Index).ToList();

            return result;
        }

        public Token GetExistingToken(string value, int index)
        {
            return ProblemContext.ProblemState.Tokens
                .FirstOrDefault(f =>
                    f.Index == index &&
                    f.Status == TokenStatusType.Correct &&
                    f.Timestamp != null);
        }

        public string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Replace(" ", "").Trim().ToLower();
        }
    }
}
