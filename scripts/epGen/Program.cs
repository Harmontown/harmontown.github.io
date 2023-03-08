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
      Guests: item.GetProperty("guests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      AudienceGuests: item.GetProperty("audienceGuests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      IsLostEpisode: item.GetProperty("isLostEpisode").GetBoolean(),
          IsTrailer: item.GetProperty("isTrailer").GetBoolean(),
          item.GetProperty("hasExplicitLanguage").GetBoolean(),
          "episode-placeholder.jpg",
          item.GetProperty("soundfile").ValueKind == JsonValueKind.Null
            ? null
            : item.GetProperty("soundfile").GetString(),
          pdId
        );

    episodes.Add(ep);
  }

  // debugging...
  episodes = episodes.Skip(60).Take(60).ToList();

  var first = episodes.First();
  var last = episodes.Last();

  // string FormatList(string[] list) => string.Join(", ", list.Select(item => "'" + HttpUtility.HtmlEncode(item) + "'"));
  string FormatList(string[] list) => string.Join(", ", list.Select(item => $"\"{HttpUtility.HtmlEncode(item)}\""));
  string FormatBool(bool value) => value.ToString().ToLower();

  foreach (var ep in episodes)
  {
    var epDir = $"{OutDir}{ep.SequenceNumber:D3}";
    Directory.CreateDirectory(epDir);
    File.WriteAllText($"{epDir}/index.md",
  $$"""
---
sequenceNumber:       {{ep.SequenceNumber}}
episodeNumber:        {{ep.EpisodeNumber}}
title:                "{{ep.Title}}"
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

venue:                "{{ep.Venue}}"
comptroller:          "{{ep.Comptroller}}"
gameMaster:           "{{ep.GameMaster}}"
hasDnD                {{FormatBool(ep.HasDnD)}}
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
    string[] Guests,
    string[] AudienceGuests,
    bool IsLostEpisode,
    bool IsTrailer,
    bool HasExplicitLanguage,
    string? Image,
    string? SoundFile,
    int? PodcastDynamiteId);
