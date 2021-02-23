using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MHWSaveTransfer.Models
{
    public class SteamApiResponse<T>
    {
        public T Response { get; set; }
    }

    public class GetPlayerSummariesResponse
    {
        public List<SteamApiPlayer> Players { get; set; }
    }

    public class ResolveVanityURLResponse
    {
        [JsonProperty("steamid")]
        public string SteamId { get; set; }

        [JsonProperty("success", Required = Required.Always)]
        public long Success { get; set; }
    }

    public class SteamApiPlayer
    {
        public string SteamId { get; set; }
        public long CommunityVisibilityState { get; set; }
        public long ProfileState { get; set; }
        public string PersonaName { get; set; }
        public long LastLogoff { get; set; }
        public Uri ProfileUrl { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
        public long PersonaState { get; set; }
        public string RealName { get; set; }
        public string PrimaryClanId { get; set; }
        public long TimeCreated { get; set; }
        public long PersonaStateFlags { get; set; }
        public string LocCountryCode { get; set; }
        public string LocStateCode { get; set; }
        public long LocCityId { get; set; }
    }

    public class SearchCommunityAjaxResponse
    {
        [JsonProperty("success", Required = Required.Always)]
        public long Success { get; set; }

        [JsonProperty("search_text", Required = Required.Always)]
        public string SearchText { get; set; }

        [JsonProperty("search_result_count", Required = Required.Always)]
        public long SearchResultCount { get; set; }

        [JsonProperty("search_filter", Required = Required.Always)]
        public string SearchFilter { get; set; }

        [JsonProperty("search_page", Required = Required.Always)]
        public long SearchPage { get; set; }

        [JsonProperty("html", Required = Required.Always)]
        public string Html { get; set; }
    }

    public class SteamUserInfo
    {
        public string AvatarUrl { get; set; }
        public string VanityURL { get; set; }
        public string PersonaName { get; set; }
        public string RealName { get; set; }
        public string Location { get; set; }
        public string? SteamId { get; set; }
    }
}
