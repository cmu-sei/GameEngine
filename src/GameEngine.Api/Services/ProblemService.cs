// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using DiscUtils.Iso9660;
using GameEngine.Abstractions;
using GameEngine.Abstractions.Models;
using GameEngine.Data;
using GameEngine.Exceptions;
using GameEngine.Extensions;
using GameEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TopoMojo.Abstractions;
using TopoMojo.Client;
using TopoMojo.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameEngine.Services
{
    public class ProblemService : DataServiceBase, IProblemService
    {
        public ProblemService(
            ILogger<ProblemService> logger,
            IOptions<Options> options,
            IGameRepository repository,
            StatsService statsService,
            ITopoMojoClient mojoClient
        ) : base(logger, options, repository)
        {
            Mojo = mojoClient;
            Macros = new List<Macro>();
            Stats = statsService;
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();
        }

        ITopoMojoClient Mojo { get; }
        List<Macro> Macros { get; }
        StatsService Stats { get; }
        private IDeserializer Deserializer;

        /// <summary>
        /// Create a new instance of a problem for a team
        /// </summary>
        /// <param name="problem"></param>
        /// <returns></returns>
        public ProblemState Spawn(Problem problem)
        {
            var context = new ProblemContext {
                Problem = problem,
                ProblemState = new ProblemState {
                    Id = problem.Id,
                    ChallengeLink = problem.ChallengeLink
                }
            };

            try
            {
                Logger.LogDebug($"initializing {problem.Id}");
                context = Initialize(problem);

                if (!Stats.ClaimSession(context.ProblemState.TeamId, "adhoc", 0))
                    throw new Exception($"All {Options.MaxSessions} sessions are in use. Please try again later.");

                var start = DateTime.UtcNow;
                var reportStat = context.Flag == null;

                Logger.LogDebug($"generating {problem.Id} [index: {problem.FlagIndex ?? -1}]");

                Generate(context);

                Logger.LogDebug($"deploying {problem.Id}");

                Deploy(context);

                if (reportStat)
                {
                    Stats.ReportStat(new ChallengeStat {
                        Id = problem.ChallengeLink.Id,
                        Sum = (int)DateTime.UtcNow.Subtract(start).TotalSeconds,
                        Count = 1
                    });
                }

                if (context.ProblemState.Start.IsEmpty())
                    context.ProblemState.Start = DateTime.UtcNow;

                if (context.ProblemState.Status != ProblemStatus.Failure
                    && context.ProblemState.Status != ProblemStatus.Success
                    && context.ProblemState.Status != ProblemStatus.Complete
                    ){
                        context.ProblemState.Status = ProblemStatus.Ready;
                    }

                Data.SaveContext(context);
            }
            catch (Exception ex)
            {
                context.ProblemState.Status = ProblemStatus.Error;
                context.ProblemState.Text = ex.Message;
                Logger.LogError(ex, $"Failed to spawn problem {problem.Id}");
            }
            finally
            {
            }

            return context.ProblemState;
        }

        private ProblemContext Initialize(Problem problem)
        {
            if (string.IsNullOrEmpty(problem?.Id))
                throw new InvalidDataException();

            var context = Data.LoadContext(problem);
            if (context == null)
                throw new ChallengeNotFoundException();

            if (!Directory.Exists(context.ProblemFolder))
                Directory.CreateDirectory(context.ProblemFolder);

            if (!Directory.Exists(context.ChallengeFolder))
                Directory.CreateDirectory(context.ChallengeFolder);

            if (context.Spec.Text.NotEmpty())
                context.Spec.Text += "\n";

            var macro = context.Macros.SingleOrDefault(m => m.Key == "{{PlayerList}}");
            if (macro == null)
            {
                context.Macros.Add(new Macro("{{PlayerList}}", () => {

                    var players = context.Problem.Team != null && context.Problem.Team.Players.NotEmpty()
                        ? context.Problem.Team.Players.Select(p => p.Name).ToList()
                        : new List<string>();
                    if (players.Count == 0)
                        players.Add("anonymous");
                    players.Sort();
                    return string.Join("", players).Replace(" ", "").ToLower();
                }));
            }

            if (context.Spec.Document.NotEmpty())
            {
                string src = Path.Combine(context.ChallengeFolder, context.Spec.Document);
                string dst = Path.Combine(context.IsoFolder, context.Spec.Document);
                if (File.Exists(src) && !File.Exists(dst))
                    File.Copy(src, dst);
            }

            return context;
        }

        private void Generate(ProblemContext context)
        {
            if (context.Flag != null)
                return;

            SelectFlag(context);

            ApplyMacros(context);

            FetchTemplates(context);

            HydrateFlagFiles(context);

            ExecuteScript(context);

            BuildIso(context);

            ResolveText(context);

            Data.SaveContext(context);
        }

        private void ResolveText(ProblemContext context)
        {
            var links = new List<string>();

            if (context.Spec.Document.NotEmpty())
                links.Add($"[PDF File]({Options.DownloadUrl}/{context.Spec.Document})");

            if (context.Flag.Iso.NotEmpty() && !context.Flag.IsoRestricted)
            {
                // try get filesize
                string fsize = "";
                string path = Path.Combine(Options.IsoPath, context.Flag.Iso);
                if (File.Exists(path))
                {
                    var info = new FileInfo(path);
                    fsize = info.Length.ToDisplayBytes();
                }
                links.Add($"[ISO File {fsize}]({Options.DownloadUrl}/{context.Flag.Iso})");
            }

            if (links.Count > 0)
            {
                var linkText = "> Download Resources: " +  string.Join(" | ", links);
                context.Spec.Text = string.Join("\n\n", linkText, context.Spec.Text);
            }

            context.Spec.Text = string.IsNullOrWhiteSpace(context.Spec.Text)
                ? string.Empty
                : Regex.Replace(context.Spec.Text, "]\\(img/", $"]({Options.DownloadUrl}/img/");

            context.Flag.Text = string.IsNullOrWhiteSpace(context.Flag.Text)
                ? string.Empty
                : Regex.Replace(context.Flag.Text, "]\\(img/", $"]({Options.DownloadUrl}/img/");

            context.ProblemState.Text = string.Join("\n", context.Spec.Text, context.Flag.Text);
        }

        private void SelectFlag(ProblemContext context)
        {
            if (context.Flag != null)
                return;

            if (!context.Problem.FlagIndex.HasValue)
            {
                context.Problem.FlagIndex = new Random().Next(context.Spec.Flags.Length);
            }

            context.FlagIndex = context.Problem.FlagIndex.Value;

            context.Flag = context.Spec.Flags[context.FlagIndex].Map<FlagSpec>();

            if (context.Flag.Workspace == null && context.Spec.Workspace != null)
                context.Flag.Workspace = context.Spec.Workspace;

            context.ProblemState.HasGamespace = context.Flag.Workspace != null;
            context.ProblemState.Tokens = GetTokens(context.Flag);
        }

        List<Token> GetTokens(FlagSpec flag)
        {
            var tokens = new List<Token>();

            if (flag != null)
            {
                var tokenSpecs = flag.Tokens;

                int index = 0;
                foreach (var ts in tokenSpecs)
                {
                    tokens.Add(new Token
                    {
                        Index = index,
                        Label = ts.Label,
                        Percent = ts.Percent,
                        Status = TokenStatusType.Pending,
                        Value = null
                    });

                    index++;
                }
            }

            return tokens;
        }


        private void ApplyMacros(ProblemContext context)
        {
            foreach (var macro in context.Macros)
            {
                if (context.Spec.Text.NotEmpty() && context.Spec.Text.Contains(macro.Key))
                    context.Spec.Text = context.Spec.Text.Replace(macro.Key, macro.Value);

                if (context.Flag.GradeInputData.NotEmpty() && context.Flag.GradeInputData.Contains(macro.Key))
                    context.Flag.GradeInputData = context.Flag.GradeInputData.Replace(macro.Key, macro.Value);
            }

            if (context.Flag.Workspace == null)
                return;

            foreach (var vm in context.Flag.Workspace.Vms)
            {
                if (vm.Replicas < 0)
                    vm.Replicas = context.Problem.Team?.Players?.Count() ?? 1;
            }
        }

        private void FetchTemplates(ProblemContext context)
        {
            if (context.Flag.Workspace?.CustomizeTemplates ?? false)
            {
                string templates = Mojo.Templates(context.Flag.Workspace.Id).Result;
                File.WriteAllText(Path.Combine(context.ProblemFolder, "_templates.json"), templates);
            }
        }

        private void BuildIso(ProblemContext context)
        {
            if (context.Flag.Iso.NotEmpty() || context.Flag.Files.IsEmpty())
                return;

            string path = Path.Combine(context.IsoFolder, context.Problem.Id + ".iso");

            try
            {
                var builder = new CDBuilder
                {
                    UseJoliet = true,
                    VolumeIdentifier = "VOL" + new Random().Next().ToString("X")
                };

                foreach (string file in context.Flag.Files)
                    builder.AddFile(Path.GetFileName(file), file);

                if (context.Spec.Document.NotEmpty())
                {
                    string docpath = Path.Combine(context.ChallengeFolder, context.Spec.Document);
                    if (!File.Exists(docpath))
                        docpath = Path.Combine(context.IsoFolder, context.Spec.Document);
                    if (File.Exists(docpath))
                        builder.AddFile(Path.GetFileName(docpath), docpath);
                }

                builder.Build(path);

                context.Flag.Iso = Path.GetFileName(path);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error building iso file for {context.Spec.Title}.");
            }
        }

        private void Deploy(ProblemContext context)
        {
            // TODO: check max-concurrent-problems

            if (context.Flag.Workspace == null)
                return;

            if (context.Flag.Iso.NotEmpty())
                context.Flag.Workspace.Iso = context.Flag.Iso;

            string fn = Path.Combine(context.ProblemFolder, "_templates.json");
            if (File.Exists(fn))
                context.Flag.Workspace.Templates = File.ReadAllText(fn);

            // temp transition mapping
            var spec = JsonConvert.DeserializeObject<TopoMojo.Models.GamespaceSpec>(
                JsonConvert.SerializeObject(context.Flag.Workspace)
            );

            spec.WorkspaceId = context.Flag.Workspace.Id;
            spec.IsolationId = context.Problem.IsolationId;

            var state = Mojo.Start(spec).Result;

            string consoleMarkdown = "> Gamespace Resources: " + String.Join(
                " | ",
                state.Vms
                    .Select(v => $"[{v.Name.Untagged()}](/console/{v.Id}/{v.Name.Untagged()}/{spec.IsolationId})")
            );

            context.ProblemState.GamespaceText = consoleMarkdown;

            string markdownMarker = "<!--tm doc-->";

            if (
                spec.AppendMarkdown
                && state.Markdown.NotEmpty()
                && !context.ProblemState.Text.Contains(markdownMarker))
            {
                context.ProblemState.Text += $"{state.Markdown}\n{markdownMarker}";
            }

            context.ProblemState.GamespaceReady = true;
            context.Flag.Workspace.Templates = null;

            Data.SaveContext(context);
        }

        private void HydrateFlagFiles(ProblemContext context)
        {
            if (context.Flag.Files.IsEmpty() || !Directory.Exists(context.ChallengeFolder))
                return;

            string[] files = Directory.GetFiles(context.ChallengeFolder, "*", SearchOption.AllDirectories);
            var selectedFiles = new List<string>();
            foreach (string target in context.Flag.Files)
            {
                if (files.Contains(target))
                {
                    selectedFiles.Add(target);
                    continue;
                }

                string match = files.FirstOrDefault(f => Path.GetFileName(f) == target);
                if (match.NotEmpty())
                {
                    selectedFiles.Add(match);
                    continue;
                }

                var rule = target.Split(":");
                if (int.TryParse(rule[0], out int count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        string[] pool = files
                            .Where(f => f.LikeGlob(rule[1]))
                            .Except(selectedFiles)
                            .ToArray();

                        if (pool.Length > 0)
                        {
                            selectedFiles.Add(pool[new Random().Next(pool.Length)]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            context.Flag.Files = selectedFiles.ToArray();
            Array.Sort(context.Flag.Files);
        }

        private void ExecuteScript(ProblemContext context)
        {
            if (context.Flag.GenerateCommand.IsEmpty())
                return;

            string[] cmds = context.Flag.GenerateCommand.Split(';');

            bool isSed = cmds.Length == 1 && cmds[0].StartsWith("sed ");

            var processInfo = isSed
                ? new ProcessStartInfo
                    {
                        FileName = "/bin/sed",
                        Arguments = context.Flag.GenerateCommand
                            .Substring(4)
                            .Replace("/dst", context.ProblemFolder)
                    }
                : new ProcessStartInfo
                    {
                        FileName = Options.Command,
                        Arguments = string.Format(
                            Options.CommandArgs,
                            context.ChallengeFolder,
                            context.ProblemFolder,
                            context.Flag.GenerateImage,
                            context.Flag.GenerateCommand
                        )
                    };

            var process = Process.Start(processInfo);

            Logger.LogInformation(process.StartInfo.Arguments);
            process.WaitForExit(Options.MaxScriptSeconds * 1000);
            if (!process.HasExited) {
                process.Kill();
                throw new ProblemGenerationTimeoutException();
            }

            if (process.ExitCode > 0)
                throw new ProblemGenerationException();

            string fileName = Path.Combine(context.ProblemFolder, context.Flag.GenerateOutputFlag);
            if (File.Exists(fileName))
            {
                string data = File.ReadAllText(fileName);
                try
                {
                    context.Flag.Tokens = Deserializer.Deserialize<TokenSpec[]>(data);
                }
                catch
                {
                    context.Flag.Tokens = new TokenSpec[]
                    {
                        new TokenSpec
                        {
                            Value = data
                        }
                    };
                }
            }

            fileName = Path.Combine(context.ProblemFolder, context.Flag.GenerateOutputText);
            if (File.Exists(fileName))
                context.Spec.Text = string.Join("\n\n", context.Spec.Text, File.ReadAllText(fileName));

            fileName = Path.Combine(context.ProblemFolder, context.Flag.GenerateOutputFileList);
            if (File.Exists(fileName))
            {
                string[] files = File.ReadAllLines(fileName);
                var additionalFiles = new List<string>();
                foreach (string file in files)
                {
                    string target = file
                        .Replace("/src", context.ChallengeFolder)
                        .Replace("/dst", context.ProblemFolder);

                    if (File.Exists(target))
                    {
                        additionalFiles.Add(target);
                    }
                }

                if (additionalFiles.Count > 0)
                {
                    context.Flag.Files = context.Flag.Files
                        .Union(additionalFiles)
                        .ToArray();
                }
            }
        }
    }
}
