// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Models
{
    public class EngineStat
    {
        public string Id { get; set; }
        public int Sum { get; set; }
        public int Count { get; set; }
        public int Average { get { return Sum / Count; } }

    }

    public class ChallengeStat: EngineStat {}

    public class StatsDump
    {
        public EngineStat[] Stats { get; set; }
        public SessionTicket[] Sessions { get; set; }
    }
}

