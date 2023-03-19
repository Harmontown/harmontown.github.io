using System.Text.Json;
using System.Linq;
using System.Web;

const string ChronologyInput = "../../data/chronology.json";
const string OutDir = "../../docs/_episodes/";

void GenerateEpisodeStubs()
{
  var episodes = JsonSerializer.Deserialize<IEnumerable<Episode>>(File.ReadAllText(ChronologyInput));

  // debugging...
  // episodes = episodes.Skip(0).Take(100).ToList();

  var first = episodes.First();
  var last = episodes.Last();

  string FormatList(string[] list, params string[] examples)
  {
    var items = (list ?? new string[0]);
    var itemLines = (list ?? new string[0]).Select(item => $"- {FormatString(item)}").ToList();
    itemLines.AddRange(examples.Skip(itemLines.Count).Select(example => $"#- {FormatString(example)}"));

    return string.Join("\n", itemLines);
  }
  string FormatBool(bool? value) => value?.ToString().ToLower() ?? "";
  string? FormatString(string? value) => value == null ? null : $"\"{value}\"";
  string? FormatDateString(string? value) => value == null ? null : FormatString(DateTimeOffset.Parse(value).ToString("u"));

  foreach (var ep in episodes)
  {
    var epDir = $"{OutDir}{ep.SequenceNumber:D3}";
    Directory.CreateDirectory(epDir);

    File.WriteAllText($"{epDir}/index.md",
  $$$"""
---
episodeNumber:        {{{ep.EpisodeNumber}}}
title:                {{{FormatString(ep.Title)}}}
image:                {{{FormatString(ep.Image)}}}
description: |-
  {{{ep.Description.Replace("\r", "").Replace("\n", "\r\n  ")}}}
showDate:             {{{FormatDateString(ep.ShowDate)}}}
releaseDate:          {{{FormatDateString(ep.ReleaseDate)}}}
duration:             {{{FormatString(ep.Duration?.ToString("c"))}}}
isLostEpisode:        {{{FormatBool(ep.IsLostEpisode)}}}
isTrailer:            {{{FormatBool(ep.IsTrailer)}}}
hasExplicitLanguage:  {{{FormatBool(ep.HasExplicitLanguage)}}}
soundFile:            {{{FormatString(ep.SoundFile)}}}

venue:                {{{FormatString(ep.Venue)}}}
comptroller:          {{{FormatString(ep.Comptroller)}}}
gameMaster:           {{{FormatString(ep.GameMaster)}}}
hasDnD:               {{{FormatBool(ep.HasDnD)}}}

external:
  harmonCity:         {{{FormatString(ep.External.HarmonCity)}}}
  podcastDynamite:
    hasMinutes:       {{{FormatBool(ep.External.PodcastDynamite.HasMinutes)}}}
    url:              {{{FormatString(ep.External.PodcastDynamite.Url)}}}
  hallOfRecords:      {{{FormatString(ep.External.HallOfRecords)}}}

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

guests:
{{{FormatList(ep.Guests, "Example guest 1", "Example guest 2")}}}

audienceGuests:
{{{FormatList(ep.AudienceGuests, "Example guest 1", "Example guest 2")}}}

images:
{{{FormatList(
  Enumerable.Empty<string>().ToArray(),
  $"/assets/images/episodes/{ep.SequenceNumber:d3}/example-1.png",
  $"/assets/images/episodes/{ep.SequenceNumber:d3}/example-2.jpeg")}}}

##############################
# Generated.  Do not change! #
##############################
layout:               episode
sequenceNumber:       {{{ep.SequenceNumber}}}
hasPrevious:          {{{ep != first}}}
hasNext:              {{{ep != last}}}
---

<!-- The episode description will be rendered here -->
{{ page.description }}

<!-- Add your content BELOW here -->
<!-- vvvvvvvvvvvvvvvvvvvvvvvvvvv -->




<!-- ^^^^^^^^^^^^^^^^^^^^^^^^^^^ -->
<!-- Add your content ABOVE here -->

<!-- The episode gallery will be rendered here -->
""");
  }
}


GenerateEpisodeStubs();



public record Episode
{
  public Episode() { }

  public Episode(
  int? episodeNumber,
  string? title,
  string? image,
  string? description,
  string? showDate,
  string? releaseDate,
  TimeSpan? duration,
  bool? isLostEpisode,
  bool? isTrailer,
  bool? hasExplicitLanguage,
  string? soundFile,
  string? venue,
  string? comptroller,
  string? gameMaster,
  bool? hasDnD,
  External external,
  string[]? guests,
  string[]? audienceGuests,
  string[]? images,
  string layout,
  int sequenceNumber,
  bool hasPrevious,
  bool hasNext
  )
  {
    EpisodeNumber = episodeNumber;
    Title = title;
    Image = image;
    Description = description;
    ShowDate = showDate;
    ReleaseDate = releaseDate;
    Duration = duration;
    IsLostEpisode = isLostEpisode;
    IsTrailer = isTrailer;
    HasExplicitLanguage = hasExplicitLanguage;
    SoundFile = soundFile;
    Venue = venue;
    Comptroller = comptroller;
    GameMaster = gameMaster;
    HasDnD = hasDnD;
    External = external;
    Guests = guests;
    AudienceGuests = audienceGuests;
    Images = images;
    Layout = layout;
    SequenceNumber = sequenceNumber;
    HasPrevious = hasPrevious;
    HasNext = hasNext;
  }

  public int? EpisodeNumber { get; init; }
  public string? Title { get; init; }
  public string? Image { get; init; }
  public string? Description { get; init; }
  public string? ShowDate { get; init; }
  public string? ReleaseDate { get; init; }
  public TimeSpan? Duration { get; init; }
  public bool? IsLostEpisode { get; init; }
  public bool? IsTrailer { get; init; }
  public bool? HasExplicitLanguage { get; init; }
  public string? SoundFile { get; init; }
  public string? Venue { get; init; }
  public string? Comptroller { get; init; }
  public string? GameMaster { get; init; }
  public bool? HasDnD { get; init; }
  public External External { get; init; }
  public string[]? Guests { get; init; }
  public string[]? AudienceGuests { get; init; }
  public string[]? Images { get; init; }
  public string Layout { get; init; }
  public int SequenceNumber { get; init; }
  public bool HasPrevious { get; init; }
  public bool HasNext { get; init; }
}

public record External
{
  public External() { }

  public External(string harmonCity, PDEntry podcastDynamite, string hallOfRecords)
  {
    HarmonCity = harmonCity;
    PodcastDynamite = podcastDynamite;
    HallOfRecords = hallOfRecords;
  }

  public string HarmonCity { get; init; }
  public PDEntry PodcastDynamite { get; init; }
  public string HallOfRecords { get; init; }
}

public record PDEntry
{
  public PDEntry() { }
  public PDEntry(bool hasMinutes, string url)
  {
    HasMinutes = hasMinutes;
    Url = url;
  }

  public bool HasMinutes { get; init; }
  public string? Url { get; init; }
}