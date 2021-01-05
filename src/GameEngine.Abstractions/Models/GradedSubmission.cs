// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace GameEngine.Models
{
    public class GradedSubmission
    {
        public string ProblemId { get; set; }
        public string SubmissionId { get; set; }
        public SubmissionStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public ProblemState State { get; set; }
        public List<Token> Tokens { get; set; } = new List<Token>();
    }

    public enum SubmissionStatus
    {
        Submitted = 0,
        Passed = 1,
        Failed = 2
    }
}
