// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using GameEngine.Abstractions;
using GameEngine.Abstractions.Models;
using GameEngine.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Client
{
    public class Proxy: IGameEngineService
    {
        public Proxy(
            HttpClient http,
            Options options/*, IGameDataCache cache*/
        )
        {
            Http = http;
            Options = options;
            //Cache = cache;
        }

        Options Options { get; }
        HttpClient Http { get; }
        //IGameDataCache Cache { get; }

        public async Task Delete(string id)
        {
            await Http.DeleteAsync($"/api/mojo/{id}");
        }

        public async Task<SessionForecast[]> GetForecast()
        {
            string data = await Http.GetStringAsync("/api/game/forecast");
            return JsonConvert.DeserializeObject<SessionForecast[]>(data);
        }

        public async Task Grade(ProblemFlag flag)
        {
            var result = await Http.PutAsync("/api/problem", Json(flag));

            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException();
        }

        //public async Task<Game> Load()
        //{
        //    string data = await Http.GetStringAsync($"/api/game/{Options.GameId}");
        //    var game = JsonConvert.DeserializeObject<Game>(data);
        //    Cache.Load(game);
        //    return game;
        //}

        public async Task<bool> ReserveSession(SessionRequest sr)
        {
            var result = await Http.PostAsync($"/api/game/reserve", Json(sr));

            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException(await result.Content.ReadAsStringAsync());

            return true;
        }

        public async Task<bool> CancelSession(string id)
        {
            var result = await Http.PostAsync($"/api/game/cancel/{id}", Json(new {}));

            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException(result.ReasonPhrase);

            return true;
        }

        public async Task Spawn(Problem problem)
        {
            var result = await Http.PostAsync("/api/problem", Json(problem));

            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException();
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            var data = await Http.GetStringAsync($"/api/mojo/vm/{vmId}/ticket");
            var info = JsonConvert.DeserializeObject<ConsoleSummary>(data);
            return info;
        }

        public async Task ChangeVm(VmAction vmAction)
        {
            var data = await Http.PutAsync($"/api/mojo/vmaction", Json(vmAction));
        }

        public async Task<ChallengeSpec> ChallengeSpec(string name)
        {
            var data = await Http.GetStringAsync($"/api/challenge/challengespec/{name}");
            var info = JsonConvert.DeserializeObject<ChallengeSpec>(data);
            return info;
        }

        public async Task<List<ChallengeSpec>> ChallengeSpecs()
        {
            var data = await Http.GetStringAsync($"/api/challenge/challengespecs");
            var info = JsonConvert.DeserializeObject<List<ChallengeSpec>>(data);
            return info;
        }

        public async Task<ChallengeSpec> SaveChallengeSpec(string name, ChallengeSpec challengeSpec)
        {
            var result = await Http.PostAsync($"/api/challenge/savechallengespec/{name}", Json(challengeSpec));

            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException(result.ReasonPhrase);

            return JsonConvert.DeserializeObject<ChallengeSpec>(await result.Content.ReadAsStringAsync());
        }

        public async Task DeleteChallengeSpec(string name)
        {
            await Http.DeleteAsync($"/api/challenge/deletechallengespec/{name}");
        }

        private HttpContent Json(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json"
            );
        }
    }
}
