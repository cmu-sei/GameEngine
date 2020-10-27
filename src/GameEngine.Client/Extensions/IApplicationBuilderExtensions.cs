// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Client;

namespace Microsoft.AspNetCore.Builder
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGameEngineCallback(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CallbackMiddleware>();
        }
    }
}

