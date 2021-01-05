// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameEngine.Api.Extensions
{
    public static class StringExtensions
    {
        public static string Unwrap(this string token, Options options)
        {
            var clean = token ?? string.Empty;

            var match = Regex.Match(clean, options.FlagWrapper, RegexOptions.IgnoreCase);
            string result = match.Success
                ? match.Groups.Values.Last().Value
                : clean;

            return result;
        }
    }
}
