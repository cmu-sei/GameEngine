// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using GameEngine.Models;
using Newtonsoft.Json;
using Xunit;

namespace GameEngine.Api.Tests
{
    public class MacroTests
    {
        /// <summary>
        /// TODO: apply this to an actual service method for testing
        /// </summary>
        [Fact]
        public void MacroKeyValueAreEqual()
        {            
            var macroFile = File.ReadAllText($"{Environment.CurrentDirectory}\\data\\macro.json");
            var macro = JsonConvert.DeserializeObject<Macro>(macroFile);

            Assert.Equal("{{PlayerList}}", macro.Key);
            Assert.Equal("Jane\nJohn\nJim\nAmber", macro.Value);
        }
    }
}

