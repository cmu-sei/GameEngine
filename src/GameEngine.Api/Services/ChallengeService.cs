// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions.Models;
using GameEngine.Api.Abstractions;
using GameEngine.Data;
using GameEngine.Extensions;
using GameEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace GameEngine.Api.Services
{
    public class ChallengeService : DataServiceBase, IChallengeService
    {
        private GameDataContext _dataContext = null;

        public ChallengeService(
            ILogger<ChallengeService> logger,
            IOptions<Options> options,
            IGameRepository repository,
            GameDataContext dataContext
        ) : base(logger, options, repository)
        {
            _dataContext = dataContext;
        }

        public ChallengeSpec GetChallengeSpec(string name)
        {
            var spec = Data.GetChallengeSpec(name);

            if (spec != null)
            {
                spec.Slug = name;
            }

            return spec;
        }

        public List<ChallengeSpec> GetChallengeSpecs()
        {
            List<ChallengeSpec> challengeSpecs = new List<ChallengeSpec>();
            var specs = Data.GetChallengeSpecs();

            foreach (KeyValuePair<string, ChallengeSpec> spec in specs)
            {
                var s = spec.Value.Map<ChallengeSpec>();
                s.Slug = spec.Key;
                challengeSpecs.Add(s);
            }

            return challengeSpecs;
        }

        public ChallengeSpec SaveChallengeSpec(string name, ChallengeSpec challengeSpec)
        {
            //bool isValid = ValidateChallengeSpec(name, challengeSpec);

            //if(!isValid)
            //{
            //    throw new ArgumentException("The ChallengeSpec was not saved. An Invalid ChallengeSpec was submitted.");
            //}

            var spec = Data.GetChallengeSpec(name);
            bool created = false;

            if (spec != null)
            {
                // this file already exists
                // archive existing spec and create a new one with this name
                if (ArchiveChallengeSpec(name))
                {
                    if (CreateNewChallengeSpec(name, challengeSpec))
                    {
                        created = true;
                    }
                }
            }
            else
            {
                if (CreateNewChallengeSpec(name, challengeSpec))
                {
                    created = true;
                }
            }

            if (created)
            {
                // TODO: force reload so we can get it from memory and return to caller. look into reloading just one item rather than all files.
                _dataContext.Reload();
                return Data.GetChallengeSpec(name);
            }

            return null;
        }

        public bool DeleteChallengeSpec(string name)
        {
            var spec = Data.GetChallengeSpec(name);

            if (spec != null)
            {
                if (Data.DeleteChallengeSpec(name))
                {
                    _dataContext.Reload();
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            else
            {
                return false;
            }
        }

        private bool ArchiveChallengeSpec(string name)
        {
            return Data.ArchiveChallengeSpec(name);      
        }

        private bool CreateNewChallengeSpec(string name, ChallengeSpec challengeSpec)
        {
            return Data.SaveChallengeSpec(name, challengeSpec);
        }
    }
}

