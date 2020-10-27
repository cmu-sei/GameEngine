// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Models
{
    public class ProblemFlag
    {
        public string Id { get; set; }
        public string[] Tokens { get; set; }
        public string SubmissionId { get; set; }
        public int Count { get; set; }        
    }
}

