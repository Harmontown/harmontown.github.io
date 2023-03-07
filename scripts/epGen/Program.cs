using System.Text.Json;

const string ChronologyInput = "../../data/chronology.json";

void GenerateEpisodeStubs()
{
  var input = File.ReadAllText(ChronologyInput);

  var doc = JsonDocument.Parse(input);

  var episodes =
    doc.RootElement.EnumerateArray()
      .Select(item =>
        new EpisodeHeader(
          item.GetProperty("sequenceNumber").GetInt32(),
          item.GetProperty("episodeNumber").ValueKind == JsonValueKind.Null
            ? null
            : item.GetProperty("episodeNumber").GetInt32(),
          item.GetProperty("title").GetString()!,
          DateTimeOffset.TryParse(item.GetProperty("showDate").GetString()!, out var showDate)
            ? showDate
            : null,
          DateTimeOffset.TryParse(item.GetProperty("releaseDate").GetString()!, out var releaseDate)
            ? releaseDate
            : null,
          TimeSpan.TryParse(item.GetProperty("duration").GetString()!, out var duration)
            ? duration
            : null,
          item.GetProperty("description").GetString()!,
          item.GetProperty("isLostEpisode").GetBoolean(),
          item.GetProperty("isTrailer").GetBoolean(),
          item.GetProperty("hasExplicitLanguage").GetBoolean(),
          "episode-placeholder.jpg",
          item.GetProperty("soundfile").ValueKind == JsonValueKind.Null 
            ? null 
            : item.GetProperty("soundfile").GetString(),
          item.GetProperty("pdId").ValueKind == JsonValueKind.Null
            ? null
            : item.GetProperty("pdId").GetInt32()
        )
      )
      .ToList();

  // debugging...
  episodes = episodes.Take(25).ToList();

  var first = episodes.First();
  var last = episodes.Last();
  foreach (var ep in episodes)
  {
    var epDir = $"../../docs/episodes/{ep.SequenceNumber:D3}";
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
showDate:             ""
publishDate:          "{{ep.ReleaseDate:u}}"
duration:             "{{ep.Duration:c}}"
isLostEpisode:        {{ep.IsLostEpisode.ToString().ToLower()}}
isTrailer:            {{ep.IsTrailer.ToString().ToLower()}}
hasExplicitLanguage:  {{ep.HasExplicitLanguage.ToString().ToLower()}}
soundFile:            {{ep.SoundFile}}

location:             
comptroller:          
guests:               []
audienceGuests:       []

# Generated.  Do not change:
layout:               episode
hasPrevious:          {{ep != first}}
hasNext:              {{ep != last}}
podcastDynamiteId:    {{ep.podcastDynamiteId}}
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
  DateTimeOffset? ShowDate,
  DateTimeOffset? ReleaseDate,
  TimeSpan? Duration,
  string Description,
  bool IsLostEpisode,
  bool IsTrailer,
  bool HasExplicitLanguage,
  string? Image,
  string? SoundFile,
  int? podcastDynamiteId);