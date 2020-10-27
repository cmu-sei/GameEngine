// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Api.Tests
{
    public class GradingContext : IDisposable
    {
        public Options Options { get; }

        public ILogger Logger { get; }

        public GradingContext()
        {
            Options = new Options();
            Logger = new TestLogger();
        }

        public ProblemContext MockProblemContext(int points, params TokenSpec[] args)
        {
            return new ProblemContext()
            {
                Spec = new ChallengeSpec
                {                     
                    
                },
                Problem = new Problem
                {
                    Id = Guid.NewGuid().ToString()
                },
                ProblemState = new ProblemState
                {
                    Tokens = new List<Token>()
                },
                Flag = new FlagSpec
                {
                    Tokens = args
                }
            };
        }

        public void Dispose()
        {
            
        }
    }
}

