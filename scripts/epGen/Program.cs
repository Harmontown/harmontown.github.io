using System.Text.Json;
using System.Linq;
using System.Web;

const string ChronologyInput = "../../data/chronology.json";
const string OutDir = "../../docs/_episodes/";

void GenerateEpisodeStubs()
{
  var chronolgy = JsonDocument.Parse(File.ReadAllText(ChronologyInput));

  var episodes = new List<EpisodeHeader>();

  foreach (var item in chronolgy.RootElement.EnumerateArray())
  {
    int? pdId =
     item.GetProperty("pdId").ValueKind == JsonValueKind.Null
        ? null
        : item.GetProperty("pdId").GetInt32();

    bool? GetBool(JsonElement el, string key) 
      => (el.GetProperty(key).ValueKind != JsonValueKind.True && el.GetProperty(key).ValueKind != JsonValueKind.False)
        ? null
        : el.GetProperty(key).GetBoolean();
    // string? GetString(JsonElement el, string key) 
    //   => el.GetProperty(key).ValueKind == JsonValueKind.Null
    //     ? null
    //     : el.GetProperty(key).GetString();

    var ep = new EpisodeHeader(
      SequenceNumber: item.GetProperty("sequenceNumber").GetInt32(),
      EpisodeNumber: item.GetProperty("episodeNumber").ValueKind == JsonValueKind.Null
        ? null
        : item.GetProperty("episodeNumber").GetInt32(),
      Title: item.GetProperty("title").GetString()!,
      Venue: item.GetProperty("venue").GetString()!,
      ShowDate: DateTimeOffset.TryParse(item.GetProperty("showDate").GetString()!, out var showDate)
        ? showDate
        : null,
      ReleaseDate: DateTimeOffset.TryParse(item.GetProperty("releaseDate").GetString()!, out var releaseDate)
        ? releaseDate
        : null,
      Duration: TimeSpan.TryParse(item.GetProperty("duration").GetString()!, out var duration)
        ? duration
        : null,
      Description: item.GetProperty("description").GetString()!,
      Comptroller: item.GetProperty("comptroller").GetString()!,
      GameMaster: item.GetProperty("gameMaster").GetString()!,
      HasDnD: GetBool(item, "hasDandD"),
      Guests: item.GetProperty("guests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      AudienceGuests: item.GetProperty("audienceGuests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      IsLostEpisode: item.GetProperty("isLostEpisode").GetBoolean(),
      IsTrailer: item.GetProperty("isTrailer").GetBoolean(),
      HasExplicitLanguage: item.GetProperty("hasExplicitLanguage").GetBoolean(),
      Image: "episode-placeholder.jpg",
      SoundFile: item.GetProperty("soundfile").GetString(),
      PodcastDynamiteId: pdId
    );

    episodes.Add(ep);
  }

  // debugging...
  // episodes = episodes.Take(120).ToList();

  var first = episodes.First();
  var last = episodes.Last();

  // string FormatList(string[] list) => string.Join(", ", list.Select(item => "'" + HttpUtility.HtmlEncode(item) + "'"));
  string FormatList(string[] list) => string.Join(", ", list.Select(value => FormatString(value)));
  string FormatBool(bool? value) => value?.ToString().ToLower() ?? "";
  string? FormatString(string? value) => value == null ? null : $"\"{HttpUtility.HtmlEncode(value)}\"";

  foreach (var ep in episodes)
  {
    var epDir = $"{OutDir}{ep.SequenceNumber:D3}";
    Directory.CreateDirectory(epDir);
    File.WriteAllText($"{epDir}/index.md",
  $$"""
---
sequenceNumber:       {{ep.SequenceNumber}}
episodeNumber:        {{ep.EpisodeNumber}}
title:                {{FormatString(ep.Title)}}
image:                {{ep.Image}}
description: >
  {{ep.Description.Replace("\n", "\n  ")}}
showDate:             "{{ep.ShowDate?.ToString("u") ?? "TBC"}}"
releaseDate:          "{{ep.ReleaseDate:u}}"
duration:             "{{ep.Duration:c}}"
isLostEpisode:        {{FormatBool(ep.IsLostEpisode)}}
isTrailer:            {{FormatBool(ep.IsTrailer)}}
hasExplicitLanguage:  {{FormatBool(ep.HasExplicitLanguage)}}
soundFile:            {{ep.SoundFile}}

venue:                {{FormatString(ep.Venue)}}
comptroller:          {{FormatString(ep.Comptroller)}}
gameMaster:           {{FormatString(ep.GameMaster)}}
hasDnD:               {{FormatBool(ep.HasDnD)}}
guests:               [{{FormatList(ep.Guests)}}]
audienceGuests:       [{{FormatList(ep.AudienceGuests)}}]

# Generated.  Do not change:
layout:               episode
hasPrevious:          {{ep != first}}
hasNext:              {{ep != last}}
podcastDynamiteId:    {{ep.PodcastDynamiteId}}
---

{% include podcastBlurb.md %}

{% comment %}
{% include people.md %}
{% endcomment %}

{% comment %}
{% include segments.md %}
{% endcomment %}

{% comment %}
{% include bits.md %}
{% endcomment %}

{% comment %}
{% include characters.md %}
{% endcomment %}

""");
  }
}


GenerateEpisodeStubs();

record EpisodeHeader(
    int SequenceNumber,
    int? EpisodeNumber,
    string Title,
    string Venue,
    DateTimeOffset? ShowDate,
    DateTimeOffset? ReleaseDate,
    TimeSpan? Duration,
    string Description,
    string Comptroller,
    string? GameMaster,
    bool? HasDnD,
    string[] Guests,
    string[] AudienceGuests,
    bool IsLostEpisode,
    bool IsTrailer,
    bool HasExplicitLanguage,
    string? Image,
    string? SoundFile,
    int? PodcastDynamiteId);
