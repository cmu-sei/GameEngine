// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using Newtonsoft.Json;

namespace GameEngine.Client.Extensions
{
    public static class MappingExtensions
    {
        public static T Map<T>(this object obj)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(obj)
            );
        }
    }
}
