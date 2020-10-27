// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Data;
using GameEngine.Extensions;
using GameEngine.Models;
using GameEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        public GameController(
            IGameRepository gameRepository,
            StatsService statsService,
            IOptions<Options> options,
            IOptions<EngineClients> clients,
            GameDataContext dataContext,
            IHttpClientFactory httpClientFactory
        )
        {
            Repository = gameRepository;
            Stats = statsService;
            Options = options.Value;
            Clients = clients.Value;
            GameDataContext = dataContext;
            HttpClientFactory = httpClientFactory;
        }

        GameDataContext GameDataContext { get; }
        IGameRepository Repository { get; }
        StatsService Stats { get; }
        Options Options { get; }
        EngineClients Clients { get; }
        IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Get a game by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpGet("{id}")]
        //public ActionResult<Game> Get(string id)
        //{
        //    return Repository.GetGame(id);
        //}

        /// <summary>
        /// Request that all configured clients reload game data
        /// </summary>
        /// <returns></returns>
        [HttpPost("reload")]
        public async Task<IActionResult> Reload()
        {
            GameDataContext.Reload();

            foreach (string client in Clients)
            {
                var http = HttpClientFactory.CreateClient(client.Untagged());
                await http.PostAsync("reload", null);
            }

            return Ok();
        }

        /// <summary>
        /// Gets a forecast of available and reserved sessions
        /// </summary>
        /// <returns></returns>
        [HttpGet("forecast")]
        public ActionResult<SessionForecast[]> GetForecast()
        {
            return Ok(Stats.SessionForecast());
        }

        /// <summary>
        /// Attempts to reserve a session if any are available
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete]
        [HttpPost("reserve/{id}")]
        public ActionResult<bool> ClaimSession([FromRoute]string id)
        {
            return ClaimSession(new SessionRequest
            {
                SessionId = id,
                MaxMinutes = 0
            });
        }

        /// <summary>
        /// Attempts to reserve a session if any are available
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("reserve")]
        public ActionResult<bool> ClaimSession([FromBody]SessionRequest sr)
        {
            bool success = Stats.ClaimSession(
                sr.SessionId,
                HttpContext.Request.Headers["X-API-CLIENT"],
                sr.MaxMinutes
            );

            if (!success)
                throw new InvalidOperationException($"All {Options.MaxSessions} sessions are in use. Please try again later.");

            return Ok(success);
        }

        /// <summary>
        /// Attempts to remove a session
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("cancel/{id}")]
        public ActionResult<bool> RemoveSession([FromRoute]string id)
        {
            bool success = Stats.RemoveSession(id);
            if (!success)
                throw new InvalidOperationException($"Session {id} could not be removed. Please try again later.");

            return Ok(success);
        }
    }
}

