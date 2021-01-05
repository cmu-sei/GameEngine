// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Client.Extensions;
using GameEngine.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Client
{
    //public interface IGameDataCache
    //{
    //    Game Game { get; }

    //    Dictionary<string, Board> Boards { get; }

    //    Dictionary<string, Board> ChallengeBoardMap { get; }

    //    void Load(Game game);
    //}

    //public class GameDataCache : IGameDataCache
    //{
    //    public Game Game { get; internal set; }

    //    public Dictionary<string, Board> Boards { get; internal set; } = new Dictionary<string, Board>();

    //    public Dictionary<string, Board> ChallengeBoardMap { get; internal set; } = new Dictionary<string, Board>();

    //    public Dictionary<string, Challenge> Challenges { get; internal set; } = new Dictionary<string, Challenge>();

    //    public void Load(Game input)
    //    {
    //        Game = input.Map<Game>();
    //        Boards = Game.Boards.ToDictionary(b => b.Id);
    //        ChallengeBoardMap.Clear();
    //        Challenges.Clear();

    //        foreach (var board in Game.Boards)
    //        {
    //            foreach (var cat in board.Categories)
    //            {
    //                foreach (var c in cat.Challenges)
    //                {
    //                    Challenges.Add(c.Id, c);
    //                    ChallengeBoardMap.Add(c.Id, board);
    //                }
    //                cat.Challenges = null;
    //            }
    //            board.Categories = null;
    //        }
    //    }
    //}
}
