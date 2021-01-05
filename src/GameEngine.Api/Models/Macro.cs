// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Extensions;
using Newtonsoft.Json;
using System;

namespace GameEngine.Models
{
    public class Macro
    {
        public string Key { get; }
        public string Value
        {
            get
            {
                if (_value.IsEmpty() && _resolve != null)
                    _value = _resolve.Invoke();

                return _value;
            }
        }

        private string _value;
        private Func<string> _resolve;

        [JsonConstructor]
        public Macro(string key, string value)
        {
            Key = key;
            _value = value;
        }

        public Macro(string key, Func<string> resolve)
        {
            Key = key;
            _resolve = resolve;
        }
    }
}
