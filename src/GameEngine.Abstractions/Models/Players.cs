// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Models
{
    public class PlayerTeam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Player[] Players { get; set; } = new Player[]{};
    }

    public class Player
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}

