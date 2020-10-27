// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Api.Abstractions;
using GameEngine.Data;
using GameEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        GameDataContext GameDataContext { get; }
        Options Options { get; }
        IChallengeService _challengeService;

        public ChallengeController(
            IChallengeService challengeService,
            IOptions<Options> options,
            GameDataContext dataContext
        )
        {
            _challengeService = challengeService;
            Options = options.Value;
            GameDataContext = dataContext;
        }

        /// <summary>
        /// Get a ChallengeSpec by slug name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("challengespec/{name}")]
        public ActionResult<ChallengeSpec> GetChallengeSpec(string name)
        {
            var challengeSpec = _challengeService.GetChallengeSpec(name);

            if (challengeSpec != null)
            {
                return Ok(challengeSpec);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get all ChallengeSpecs
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("challengespecs")]
        public ActionResult<List<ChallengeSpec>> GetChallengeSpecs()
        {
            var challengeSpecs = _challengeService.GetChallengeSpecs();

            if (challengeSpecs != null)
            {
                return Ok(challengeSpecs);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Add or update a challenge spec
        /// </summary>
        /// <param name="name">The name of the challenge spec</param>
        /// <param name="challengeSpec">The ChallengeSpec object to save</param>
        /// <returns></returns>
        [HttpPost("savechallengespec/{name}")]
        public ActionResult<ChallengeSpec> SaveChallengeSpec([FromRoute]string name, [FromBody]ChallengeSpec challengeSpec)
        {
            ChallengeSpec spec = _challengeService.SaveChallengeSpec(name, challengeSpec);

            if (spec == null)
            {
                throw new InvalidOperationException($"ChallengeSpec {name} was not saved. Please try again.");
            }

            return Ok(spec);
        }

        /// <summary>
        /// Delete a challenge spec
        /// </summary>
        /// <param name="name">The name of the challenge spec</param>
        /// <returns></returns>
        [HttpDelete("deletechallengespec/{name}")]
        public ActionResult DeleteChallengeSpec([FromRoute]string name)
        {
            bool deleted = _challengeService.DeleteChallengeSpec(name);

            if (!deleted)
            {
                throw new InvalidOperationException($"ChallengeSpec {name} was not deleted. Please try again.");
            }

            return new NoContentResult();
        }
    }
}

