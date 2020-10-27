// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Abstractions.Models
{
    public class VmSpec
    {
        public string Name { get; set; }
        public int Replicas { get; set; }
        public bool SkipIso { get; set; }
    }
}

