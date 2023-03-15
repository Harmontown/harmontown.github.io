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
      HasDnD: GetBool(item, "hasDnD"),
      Guests: item.GetProperty("guests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      AudienceGuests: item.GetProperty("audienceGuests").EnumerateArray().Select(item => item.GetString()).ToArray()!,
      IsLostEpisode: item.GetProperty("isLostEpisode").GetBoolean(),
      IsTrailer: item.GetProperty("isTrailer").GetBoolean(),
      HasExplicitLanguage: item.GetProperty("hasExplicitLanguage").GetBoolean(),
      Image: "episode-placeholder.jpg",
      SoundFile: item.GetProperty("soundfile").GetString(),
      HarmonCityUrl: item.GetProperty("harmonCityUrl").GetString(),
      PodcastDynamiteUrl: item.GetProperty("podcastDynamiteUrl").GetString(),
      HasMinutes: GetBool(item, "hasMinutes"),
      HallOfRecordsUrl: item.GetProperty("hallOfRecordsUrl").GetString()
    );

    episodes.Add(ep);
  }

  // debugging...
  // episodes = episodes.Skip(0).Take(100).ToList();

  var first = episodes.First();
  var last = episodes.Last();

  string FormatList(string[] list)
  {
    if (list.Length == 0)
    {
      return string.Empty;
    }
    return $"\n- {string.Join("\n- ", list.Select(value => FormatString(value)))}";
  }
  string ExampleIfEmpty(string[] list, params string[] examples)
  {
    if (list.Length != 0)
    {
      return string.Empty;
    }
    return $"\n#- {string.Join("\n#- ", examples.Select(value => FormatString(value)))}";
  }
  string FormatBool(bool? value) => value?.ToString().ToLower() ?? "";
  string? FormatString(string? value) => value == null ? null : $"\"{HttpUtility.HtmlEncode(value)}\"";

  foreach (var ep in episodes)
  {
    var epDir = $"{OutDir}{ep.SequenceNumber:D3}";
    Directory.CreateDirectory(epDir);
    File.WriteAllText($"{epDir}/index.md",
  $$"""
---
episodeNumber:        {{ep.EpisodeNumber}}
title:                {{FormatString(ep.Title)}}
image:                {{FormatString(ep.Image)}}
description: >
  {{ep.Description.Replace("\r", "").Replace("\n", "\r\n  ")}}
showDate:             {{FormatString(ep.ShowDate?.ToString("u"))}}
releaseDate:          {{FormatString(ep.ReleaseDate?.ToString("u"))}}
duration:             {{FormatString(ep.Duration?.ToString("c"))}}
isLostEpisode:        {{FormatBool(ep.IsLostEpisode)}}
isTrailer:            {{FormatBool(ep.IsTrailer)}}
hasExplicitLanguage:  {{FormatBool(ep.HasExplicitLanguage)}}
soundFile:            {{FormatString(ep.SoundFile)}}

venue:                {{FormatString(ep.Venue)}}
comptroller:          {{FormatString(ep.Comptroller)}}
gameMaster:           {{FormatString(ep.GameMaster)}}
hasDnD:               {{FormatBool(ep.HasDnD)}}

external:
  harmonCity:         {{FormatString(ep.HarmonCityUrl)}}
  podcastDynamite:
    hasMinutes:       {{FormatBool(ep.HasMinutes)}}
    url:              {{FormatString(ep.PodcastDynamiteUrl)}}
  hallOfRecords:      {{FormatString(ep.HallOfRecordsUrl)}}

# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
# Tip!
#   If a line starts with the # symbold then it is considered a comment.
#   Comments do not get processed by the wiki.  They are purely for your information.
# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
# Tip!
#   Adding items to lists is easy.  List items always start with a - symbol and have
#   have the same identation as the list name.  Here is an example.
#
#    foods:
#    - "bacon"
#    - "sausages"
#
#   In this example the list name is "foods" and it has two items (bacon, and sausages).
#
#   To get you started, the "guests", "audienceGuests", and "images" lists below have
#   a few example entries commented out.
#   To start using them remove the # symbol from the start of the line.
#
# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

guests:{{FormatList(ep.Guests)}}{{ExampleIfEmpty(ep.Guests, "Example guest 1", "Example guest 2")}}

audienceGuests:{{FormatList(ep.AudienceGuests)}}{{ExampleIfEmpty(ep.AudienceGuests, "Example guest 1", "Example guest 2")}}

images:{{ExampleIfEmpty(
  Enumerable.Empty<string>().ToArray(),
  $"/assets/images/episodes/{ep.SequenceNumber:d3}/example-1.png",
  $"/assets/images/episodes/{ep.SequenceNumber:d3}/example-2.jpeg")}}

##############################
# Generated.  Do not change! #
##############################
layout:               episode
sequenceNumber:       {{ep.SequenceNumber}}
hasPrevious:          {{ep != first}}
hasNext:              {{ep != last}}
---

<!-- The episode description will be rendered here -->

<!-- Add your content BELOW here -->
<!-- vvvvvvvvvvvvvvvvvvvvvvvvvvv -->




<!-- ^^^^^^^^^^^^^^^^^^^^^^^^^^^ -->
<!-- Add your content ABOVE here -->

<!-- The episode gallery will be rendered here -->
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
    string? HarmonCityUrl,
    string? PodcastDynamiteUrl,
    bool? HasMinutes,
    string? HallOfRecordsUrl
    );
