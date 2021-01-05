// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;

namespace GameEngine.Abstractions.Models
{
    public class ChallengeSpec
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Authors { get; set; }
        public string Tags { get; set; }
        public string Text { get; set; }
        public string Document { get; set; }
        public int Difficulty { get; set; }
        public FlagStyle FlagStyle { get; set; }
        public FlagSpec[] Flags { get; set; } = new FlagSpec[] { };
        public WorkspaceSpec Workspace { get; set; }
        public bool IsMultiStage { get; set; }
        public bool IsMultiPart { get; set; }
    }

    public enum FlagStyle
    {
        Token,
        Text
    }
}
