using System.Xml.Linq;

const string TidiedRssInput = "../../data/rss-tidied.xml";
const string chronologyInput = "../../data/chronology.json";

void GenerateEpisodeStubs()
{
  var input = File.ReadAllText(TidiedRssInput);

  var doc = XDocument.Parse(input);

  var episodes =
    doc
      .Descendants("item")
      .OrderBy(item => DateTimeOffset.Parse(item.Element("pubDate")!.Value))
      .Select((item, i) =>
      {
        var ep = new EpisodeHeader(
          i + 1,
          item.Element("title")!.Value,
          DateTimeOffset.Parse(item.Element("pubDate")!.Value),
          TimeSpan.FromSeconds(int.Parse(item.Element("duration")!.Value)),
          item.Element("description")!.Value,
          item.Element("episodeType")!.Value,
          item.Element("explicit")?.Value == "yes",
          // item.Element("image") != null ? new Uri(item.Element("image")?.Attribute("href")?.Value ?? "") : null,
          "episode-placeholder.jpg",
          new Uri(item.Element("url")!.Value));
        return ep;
      })
      .ToList();

  // debugging...
  episodes = episodes.Take(5).ToList();

  var first = episodes.First();
  var last = episodes.Last();
  foreach (var ep in episodes)
  {
    var epDir = $"../../docs/episodes/{ep.Number:D3}";
    Directory.CreateDirectory(epDir);
    File.WriteAllText($"{epDir}/index.md",
$$"""
---
number:               {{ep.Number}}
title:                "{{ep.Title}}"
image:                {{ep.Image}}
description: >
  {{ep.Description.Replace("\n", "\n  ")}}
showDate:             ""
publishDate:          "{{ep.PublishDate:u}}"
duration:             "{{ep.Duration:c}}"
episodeType:          {{ep.Type}}
hasExplicitLanguage:  {{ep.HasExplicitLanguage}}
soundFile:            {{ep.SoundFile}}

location:             
comptroller:          
guests:               []
audienceGuests:       []

layout:               episode
hasPrevious:          {{ep != first}}
hasNext:              {{ep != last}}
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
  int Number,
  string Title,
  DateTimeOffset PublishDate,
  TimeSpan Duration,
  string Description,
  string Type,
  bool HasExplicitLanguage,
  string? Image,
  Uri SoundFile);