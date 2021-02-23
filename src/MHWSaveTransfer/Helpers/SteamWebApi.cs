using AngleSharp;
using MHWSaveTransfer.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MHWSaveTransfer.Helpers
{
    public class SteamWebApi
    {
        private int errorCount = 0;
        private readonly string apiKey;
        private readonly HttpClient client;

        private readonly string sessionId;
        private readonly CookieContainer cookieContainer;
        private readonly HttpClient cookieClient;
        private readonly IBrowsingContext htmlParser;

        public SteamWebApi(string apiKey)
        {
            this.apiKey = apiKey;
            client = new HttpClient();

            // For use with SearchSteamUser
            sessionId = GenerateSessionId();
            cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("sessionid", sessionId, "/", "steamcommunity.com"));
            cookieClient = new HttpClient(new HttpClientHandler() { CookieContainer = cookieContainer });
            htmlParser = BrowsingContext.New(Configuration.Default);
        }

        public async Task<string> GetPersonaName(string steamId)
        {
            if (errorCount < 3)
            {
                // Yeah it sucks but who cares
                try
                {
                    var res = await client.GetAsync("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + apiKey + "&steamids=" + steamId);
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var obj = JsonConvert.DeserializeObject<SteamApiResponse<GetPlayerSummariesResponse>>(json);
                        return obj.Response.Players.FirstOrDefault()?.PersonaName ?? "(no account found)";
                    }
                    else
                    {
                        errorCount++;
                        throw new Exception(res.ReasonPhrase);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{ex.Message}. ErrorCount: {errorCount}/3");
                }
            }

            return "(steam api error)";
        }

        public async Task<string> GetSteamId(string vanityurl)
        {
            if (errorCount < 3)
            {
                // Yeah it sucks but who cares
                try
                {
                    var res = await client.GetAsync("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + apiKey + "&vanityurl=" + vanityurl);
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var obj = JsonConvert.DeserializeObject<SteamApiResponse<ResolveVanityURLResponse>>(json);

                        // If there is no SteamId returned then the vanityurl probably already IS the steamId.
                        // I say probably, because there is also a chance that the account really doesn't exist.
                        // For my spaghetti use-cases this works well enough though.
                        return obj.Response.SteamId ?? vanityurl;
                    }
                    else
                    {
                        errorCount++;
                        throw new Exception(res.ReasonPhrase);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{ex.Message}. ErrorCount: {errorCount}/3");
                }
            }

            return "(steam api error)";
        }

        public async Task<List<SteamUserInfo>> SearchSteamUser(string query, CancellationToken ct = default)
        {
            // Steam doesn't have an API for this, so yeah...

            var escaped = Uri.EscapeUriString(query);
            var requestUri = $"https://steamcommunity.com/search/SearchCommunityAjax?text={query}&filter=users&sessionid={sessionId}";
            var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var res = await cookieClient.SendAsync(req, ct);

            var contentString = await res.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<SearchCommunityAjaxResponse>(contentString).Html;
            var doc = await htmlParser.OpenAsync(x => x.Content(content), ct);
            var resultRows = doc.QuerySelectorAll(".search_row");

            var lst = new List<SteamUserInfo>();

            foreach (var row in resultRows)
            {
                var avatarUrl = row.QuerySelector("a > img").GetAttribute("src");
                var profileUrl = row.QuerySelector(".searchPersonaName").GetAttribute("href");
                var lastSlashIdx = profileUrl.LastIndexOf('/');

                if (lastSlashIdx == -1)
                    throw new Exception("Couldn't decode profileUrl.");

                var profileKey = profileUrl.Substring(lastSlashIdx + 1);

                var personaInfo = row.QuerySelector(".searchPersonaInfo")
                    .TextContent
                    .Split('\t')
                    .Where(x => x.Length > 1)
                    .ToArray();

                var personaName = personaInfo.ElementAtOrDefault(0)?.Trim() ?? "";
                var realName = personaInfo.ElementAtOrDefault(1)?.Trim() ?? "";
                var location = personaInfo.ElementAtOrDefault(2)?.Trim() ?? "";

                lst.Add(new SteamUserInfo
                {
                    AvatarUrl = avatarUrl,
                    VanityURL = profileKey,
                    PersonaName = personaName,
                    RealName = realName,
                    Location = location
                });
            }

            return lst;
        }

        private static string GenerateSessionId()
        {
            var chars = "abcdef0123456789";
            var stringChars = new char[24];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}
