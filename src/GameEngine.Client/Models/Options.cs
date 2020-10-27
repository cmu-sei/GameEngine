// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

namespace GameEngine.Client
{
    public class Options
    {
        public string GameId { get; set; }
        public string GameEngineUrl { get; set; }
        public string GameEngineKey { get; set; }
        public string LocalPath { get; set; } = "./";
        public string CallbackEnpoint { get; set; } = "/api/engine/";
        public int MaxRetries { get; set; } = 2;
    }
}

