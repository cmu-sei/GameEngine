// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Api.Grading;
using GameEngine.Models;
using System;
using System.Linq;
using Xunit;

namespace GameEngine.Api.Tests
{
    public class GradingStrategyTests
    {
        [Fact]
        public void MatchSucceedsWithCorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "1111" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "2222" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "1111", "2222" }
                };

                var match = new Match(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }

        [Fact]
        public void MatchSucceedsWithDenormalizedCorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "1ab       C    1" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "2xyZ             2" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "1  A  B  C  1", "2  X  Y  Z  2" }
                };

                var match = new Match(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }

        [Fact]
        public void MatchFailsWithIncorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100, 
                    new TokenSpec { Label = "1", Percent = 50, Value = "1111" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "2222" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "aaaa", "bbbb" }
                };

                var match = new Match(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);
            }
        }

        [Fact]
        public void MatchFailsWithPartialCredit()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 60, Value = "1111" },
                    new TokenSpec { Label = "2", Percent = 40, Value = "2222" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "aaaa", "2222" }
                };

                var match = new Match(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);

                Assert.Equal(40, grade.CorrectPercent);
            }
        }

        [Fact]
        public void MatchAllSucceedsWithCorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "THREE|TWO|ONE" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "SIX|FIVE|FOUR" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "one three two", "four six five" }
                };

                var match = new MatchAll(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }



        [Fact]
        public void MatchAllFailsWithIncorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "THREE|TWO|ONE" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "SIX|FIVE|FOUR" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "one, two, four", "three five six" }
                };

                var match = new MatchAll(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);
            }
        }

        [Fact]
        public void MatchAllFailsWithPartialCredit()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 60, Value = "THREE|TWO|ONE" },
                    new TokenSpec { Label = "2", Percent = 40, Value = "SIX|FIVE|FOUR" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "one is smaller than two is smaller than three", "four is less than five is less than seven" }
                };

                var match = new MatchAll(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);

                Assert.Equal(60, grade.CorrectPercent);
            }
        }

        [Fact]
        public void MatchAlphaNumericSucceedsWithCorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "1111Aa" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "2222Bb" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "!$!@$!$%!%1111^%$^$%^$%^$%^Aa", "#@%$@#%^@#^$#$^2222@^#$^@$!@$!$Bb" }
                };

                var match = new MatchAlphaNumeric(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }

        [Fact]
        public void MatchAlphaNumericSucceedsWithCorrectSingleValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 100, Value = "123.456-7&8" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "..123~456~78" }
                };

                var match = new MatchAlphaNumeric(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }

        [Fact]
        public void MatchAlphaNumericFailsWithIncorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "1111" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "2222" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "@%@#%@#%@#%@^aaaa^^#@$^@$!#$!", "#%@^@#$^@#$@#$bbbb@%^@#^@%^@#$%@#" }
                };

                var match = new MatchAlphaNumeric(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);
            }
        }

        [Fact]
        public void MatchAlphaNumericFailsWithPartialCredit()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 10, Value = "1111Aa" },
                    new TokenSpec { Label = "2", Percent = 90, Value = "2222Bb" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "^%@$#^$%@#^$%@^$@^#$@%#$%#$@%#@$%", "2222!Bb" }
                };

                var match = new MatchAlphaNumeric(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);

                Assert.Equal(90, grade.CorrectPercent);
            }
        }

        [Fact]
        public void MatchAnySucceedsWithCorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 50, Value = "ONE|TWO|THREE" },
                    new TokenSpec { Label = "2", Percent = 50, Value = "FOUR|FIVE|SIX" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "three", "five" }
                };

                var match = new MatchAny(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.True(grade.Success);
            }
        }

        [Fact]
        public void MatchAnyFailsWithIncorrectValues()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 10, Value = "1|2" },
                    new TokenSpec { Label = "2", Percent = 90, Value = "3|4" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "AB", "CD" }
                };

                var match = new MatchAny(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);
            }
        }

        [Fact]
        public void MatchAnyFailsWithPartialCredit()
        {
            using (var context = new GradingContext())
            {
                var problemContext = context.MockProblemContext(100,
                    new TokenSpec { Label = "1", Percent = 10, Value = "OnE|TWO|ThReE" },
                    new TokenSpec { Label = "2", Percent = 90, Value = "4|5|6" }
                );

                var problemFlag = new ProblemFlag
                {
                    Id = Guid.NewGuid().ToString(),
                    SubmissionId = Guid.NewGuid().ToString(),
                    Tokens = new string[] { "two", "J" }
                };

                var match = new MatchAny(context.Options, context.Logger, problemContext);

                var grade = match.GradeTokens(problemFlag);

                Assert.False(grade.Success);
                
                Assert.Equal(10, grade.CorrectPercent);
            }
        }

        [Fact(Skip = "Need to figure out a good test case")]
        public void MatchOutputSucceedsWithCorrectValues()
        {            
        }

        [Fact(Skip = "Need to figure out a good test case")]
        public void MatchOutputFailsWithIncorrectValues()
        {            
        }

        [Fact(Skip = "Need to figure out a good test case")]
        public void MatchOutputFailsWithPartialCredit()
        {
            
        }
    }
}

