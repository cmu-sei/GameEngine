// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameEngine.Api.Controllers
{
    /// <summary>
    /// home index and api
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        public HomeController()
        {
        }

        /// <summary>
        /// root
        /// </summary>
        /// <returns></returns>
        [HttpGet("/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = new HomeModel
            {
                ApplicationName = "Game Engine"
            };

            return View(model);
        }
    }
}
