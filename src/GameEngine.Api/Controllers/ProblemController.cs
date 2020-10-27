// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Data;
using GameEngine.Extensions;
using GameEngine.Models;
using GameEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly IQueueService<ClientProblem> _problemQueue;
        private readonly IQueueService<ClientProblemFlag> _flagQueue;
        private readonly IGameRepository _repo;
        StatsService Stats { get; }
        ILogger Logger { get; }

        public ProblemController(
            IQueueService<ClientProblem> problemService,
            IQueueService<ClientProblemFlag> flagQueue,
            IGameRepository repository,
            ILogger<ProblemController> logger,
            StatsService statService
        )
        {
            _problemQueue = problemService;
            _flagQueue = flagQueue;
            _repo = repository;
            Logger = logger;
            Stats = statService;
        }

        /// <summary>
        /// Get ProblemState by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<ProblemState> Get(string id)
        {
            var state = _repo.GetProblem(id);
            if (state == null)
                return NotFound();

            return Ok(state);
        }

        /// <summary>
        /// Create an instance of a Problem
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] Problem model)
        {
            var qm = new ClientProblem
            {
                Id = model.Id,
                Client = HttpContext.Request.Headers["X-API-CLIENT"],
                CallbackUrl = HttpContext.Request.Headers["X-API-CALLBACK"],
                Problem = model
            };

            _problemQueue.Enqueue(qm);

            return Created("", null);
        }

        /// <summary>
        /// Update an instance of a Problem
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public IActionResult Put([FromBody] ProblemFlag model)
        {
            var qm = new ClientProblemFlag
            {
                Id = model.Id,
                Client = HttpContext.Request.Headers["X-API-CLIENT"],
                CallbackUrl = HttpContext.Request.Headers["X-API-CALLBACK"],
                ProblemFlag = model
            };

            _flagQueue.Enqueue(qm);

            return Ok();
        }

        #region DEV Enpoints

        // These endpoints simulate a remote Gameboard,
        // for isolated testing

        [HttpPost("/api/engine/updated")]
        public IActionResult Updated([FromBody] ProblemState model)
        {
            model.Log(Logger);
            return Ok();
        }

        [HttpPost("/api/engine/graded")]
        public IActionResult Graded([FromBody] GradedSubmission model)
        {
            model.Log(Logger);
            return Ok();
        }

        [HttpPost("/api/engine/reload")]
        public IActionResult Reload()
        {
            Logger.LogInformation("Received reload event.");
            return Ok();
        }

        #endregion
    }
}

