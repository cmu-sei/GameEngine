// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Abstractions.Models
{
    public class WorkspaceSpec
    {
        public int Id { get; set; }
        public NetworkSpec Network { get; set; }
        public VmSpec[] Vms { get; set; } = new VmSpec[] {};
        public bool CustomizeTemplates { get; set; }
        public string Templates { get; set; }
        public string Iso { get; set; }
        public string IsoTarget { get; set; }
        public bool HostAffinity { get; set; }
        public bool AppendMarkdown { get; set; }
    }
}

