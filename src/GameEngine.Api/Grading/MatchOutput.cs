// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Exceptions;
using GameEngine.Extensions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameEngine.Api.Grading
{
    public class MatchOutput : GradingStrategy
    {
        public MatchOutput(Options options, ILogger logger, ProblemContext problemContext)
            : base(options, logger, problemContext) { }

        public override bool Unwrap => false;

        public override bool GradeToken(TokenSpec spec, string token)
        {            
            if (ProblemContext.Flag.GradeCommand.IsEmpty())
                throw new GradeCommandEmptyException();

            File.WriteAllText(
                    Path.Combine(ProblemContext.ProblemFolder, ProblemContext.Flag.GradeInputFlag),
                    token
                );

            if (ProblemContext.Flag.GradeInputFile.NotEmpty())
            {
                File.WriteAllText(
                    Path.Combine(ProblemContext.ProblemFolder, ProblemContext.Flag.GradeInputFile),
                    ProblemContext.Flag.GradeInputData
                );
            }

            var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = Options.Command,
                    Arguments = string.Format(
                        Options.CommandArgs,
                        ProblemContext.ChallengeFolder,
                        ProblemContext.ProblemFolder,
                        ProblemContext.Flag.GradeImage,
                        ProblemContext.Flag.GradeCommand
                    )
                }
            );

            Logger.LogDebug(process.StartInfo.Arguments);
            int timeout = ProblemContext.Flag.GradeCommandTimeout > 0 
                ? ProblemContext.Flag.GradeCommandTimeout 
                : Options.MaxScriptSeconds;

            process.WaitForExit(timeout * 1000);
            if (!process.HasExited)
            {
                process.Kill();
                throw new ProblemGradingTimeoutException();
            }

            var fileName = Path.Combine(ProblemContext.ProblemFolder, ProblemContext.Flag.GradeOutputFile);

            if (File.Exists(fileName))
            {
                return Normalize(File.ReadAllText(fileName)) == Normalize(spec.Value);
            }

            return false;
        }
    }
}

