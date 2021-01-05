// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;

namespace GameEngine.Models
{
    public class ClientProblem : IClientProblem
    {
        public string Id { get; set; }
        public string Client { get; set; }
        public string CallbackUrl { get; set; }
        public Problem Problem { get; set; }
    }
}
