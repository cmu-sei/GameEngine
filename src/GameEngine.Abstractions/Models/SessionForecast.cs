// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;

namespace GameEngine.Models
{
    public class SessionForecast
    {
        public DateTime Time { get; set; }
        public int Available { get; set; }
        public int Reserved { get; set; }
    }
}
