// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GameEngine.Extensions
{
    public static class ProblemContextExtensions
    {

        public static bool NotEmpty(this string s)
        {
            return !String.IsNullOrEmpty(s);
        }

        public static bool IsEmpty(this string s)
        {
            return String.IsNullOrEmpty(s);
        }

        public static bool NotEmpty(this object[] a)
        {
            return a != null && a.Length > 0;
        }

        public static bool IsEmpty(this object[] a)
        {
            return a == null || a.Length == 0;
        }

        public static bool IsEmpty(this DateTime dt)
        {
            return dt == DateTime.MinValue;
        }

        public static bool IsEmpty(this DateTime? dt)
        {
            return dt == null || dt == DateTime.MinValue;
        }

        public static string Untagged(this string s)
        {
            return s.Split("#")[0];
        }

        public static string Truncate(this string s, int count)
        {
            return s.Length > count
                ? s.Substring(0, count) + "..."
                : s;
        }

        public static bool LikeGlob(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }

        public static string ToHash(this string str)
        {
            return BitConverter.ToString(
                SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(str))
            ).Replace("-", "").ToLower();
        }

        public static string ToDisplayBytes(this long value)
        {
            var tags = new string[] { "B", "KB", "MB", "GB", "TB", "PB"};
            string f = "F0";
            int i = 0;
            double v = value;
            double n = v / 1024;

            while (n >= 1)
            {
                f = $"F{i}";
                v = n;
                n = v / 1024;
                i += 1;
            }
            return $"{v.ToString(f)}{tags[i]}";
        }
    }
}

