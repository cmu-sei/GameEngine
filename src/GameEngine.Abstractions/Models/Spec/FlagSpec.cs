// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Abstractions.Models
{
    public class FlagSpec
    {
        public FlagType Type { get; set; } = FlagType.Match;

        /// <summary>
        /// TODO: What is this field for?
        /// </summary>
        public int Value { get; set; } = 1;        
        public TokenSpec[] Tokens { get; set; } = new TokenSpec[] { };
        public string GenerateOutputText { get; set; } = ".gen_text";
        public string GenerateOutputFlag { get; set; } = ".gen_flag";
        public string GenerateOutputFileList { get; set; } = ".gen_files";
        public string GenerateCommand { get; set; }
        public string GenerateImage { get; set; } = "bash";
        public string GradeInputFlag { get; set; } = ".grade_flag";
        public string GradeInputData { get; set; }
        public string GradeInputFile { get; set; } = ".grade_in";
        public string GradeOutputFile { get; set; } = ".grade_out";
        public string GradeCommand { get; set; }
        public int GradeCommandTimeout { get; set; }
        public string GradeImage { get; set; }
        public WorkspaceSpec Workspace { get; set; }
        public string[] Files { get; set; } = new string[] { };
        public string Iso { get; set; }
        public bool IsoRestricted { get; set; }
        public string Text { get; set; }
    }

    public enum FlagType
    {
        Match,
        MatchAll,
        MatchAny,
        MatchOutput,
        MatchAlphaNumeric
    }
}

