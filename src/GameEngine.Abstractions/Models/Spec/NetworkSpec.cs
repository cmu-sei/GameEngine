// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Abstractions.Models
{
    public class NetworkSpec
    {
        public string[] Hosts { get; set; }
        public string NewIp { get; set; }
        public string[] Dnsmasq { get; set; }
    }
}
