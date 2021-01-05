// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Extensions;
using GameEngine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TopoMojo.Abstractions;

namespace GameEngine.Api.Controllers
{
    public class MojoController : ControllerBase
    {
        public MojoController(
            ITopoMojoClient mojo,
            IOptions<Options> options
        )
        {
            Mojo = mojo;
            Options = options.Value;
        }

        ITopoMojoClient Mojo { get; }
        Options Options { get; }

        /// <summary>
        /// Stop and delete an instance of a virtual machine
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("/api/mojo/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await Mojo.Stop(id);
            return Ok();
        }

        /// <summary>
        /// Get's a one-time access ticket to a vm console
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/mojo/vm/{id}/ticket")]
        public async Task<ActionResult<ConsoleSummary>> GetTicket([FromRoute]string id)
        {
            var info = await Mojo.Ticket(id);
            if (info != null && info.Url.NotEmpty() && Options.ConsoleUrlTransformKey.NotEmpty() && Options.ConsoleUrlTransformValue.NotEmpty())
                info.Url = info.Url.Replace(Options.ConsoleUrlTransformKey, Options.ConsoleUrlTransformValue);
            return Ok(info);
        }

        /// <summary>
        /// Restart a virtual machine
        /// </summary>
        /// <param name="vmAction"></param>
        /// <returns></returns>
        [HttpPut("/api/mojo/vmaction")]
        public async Task<ActionResult> RestartVm([FromBody]VmAction vmAction)
        {
            var action = JsonConvert.DeserializeObject<TopoMojo.Models.VmAction>(
                JsonConvert.SerializeObject(vmAction)
            );
            await Mojo.ChangeVm(action);
            return Ok();
        }
    }
}
