// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Abstractions.Models
{
    public class TokenSpec
    {
        public string Value { get; set; }
        public int Percent { get; set; }
        //public int? Index { get; set; }
        public string Label { get; set; }
    }
}
