using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

const string PDInput = "../../data/raw/podcastDynamite/";
const string RssInput = "../../data/rss-tidied.xml";
const string Output = "../../data/chronology.json";

void DeriveChronology()
{
  const int PDLast = 377;
  const int rssLast = 364;

  var entries = new List<object>();

  int pdId = 0;
  int rssIndex = 0;
  int sequenceNumber = 0;
  int episodeNumber = 0;

  var isPDOver = false;
  var isRssOver = false;

  var rssDoc = XDocument.Parse(File.ReadAllText(RssInput));
  var rssItems = rssDoc.Descendants("item").Reverse().ToList();

  while (!isPDOver || !isRssOver)
  {
    sequenceNumber++;

    PDItem? pdItem = null;
    if (!isPDOver)
    {
      pdId++;
      var doc = JsonDocument.Parse(File.ReadAllText($"{PDInput}{pdId:D3}/episode.json"));
      pdItem = new PDItem(doc.RootElement);
    }

    // fudge
    if (pdItem?.Title == "HARMONTOWN NIGHTS PREVIEW")
    {
      pdItem = pdItem with { EpisodeNumber = 98 };
    }

    XElement? xDoc = null;
    if (pdItem?.EpisodeNumber != null)
    {
      episodeNumber = pdItem.EpisodeNumber.Value;
      xDoc = rssItems[rssIndex];
      rssIndex++;
    }

    if (isPDOver && !isRssOver)
    {
      xDoc = rssItems[rssIndex];
      rssIndex++;
      episodeNumber++;
    }

    var rssItem = xDoc == null ? null : new RssItem(xDoc!, episodeNumber);
    var entry = new
    {
      sequenceNumber = sequenceNumber,
      pdId = isPDOver ? (int?)null : pdId,
      rssIndex = rssItem == null ? (int?)null : rssIndex,
      episodeNumber = (pdItem?.IsLostEpisode ?? false) ? (int?)null : episodeNumber,
      isLostEpisode = pdItem?.IsLostEpisode ?? false,
      isTrailer = rssItem?.IsTrailer ?? false,
      title = rssItem?.Title ?? pdItem?.Title ?? "",
      image = "episode-placeholder.jpg",
      description = rssItem?.Description ?? pdItem?.Description ?? "",
      showDate = pdItem?.ShowDate,
      releaseDate = rssItem?.ReleaseDate ?? pdItem?.ReleaseDate,
      duration = rssItem?.Duration ?? pdItem?.Duration,
      hasExplicitLanguage = rssItem?.HasExplicitLanguage ?? false,
      soundfile = rssItem?.SoundFile,
    };

    entries.Add(entry);

    isPDOver = pdId >= PDLast;
    isRssOver = rssIndex >= rssLast;
  }

  var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions() { WriteIndented = true });

  File.WriteAllText(Output, json);
}

DeriveChronology();

record PDItem
{
  public PDItem(JsonElement docRoot)
  {
    var episodeNumberExtractor = new Regex("^(\\d{1,3}) -");

    this.Title = docRoot.GetProperty("title").GetString()!;
    this.IsLostEpisode = this.Title.StartsWith("Lost Episode #");
    this.Description = docRoot.GetProperty("description").GetString()!;

    var episodeNumberString =
      episodeNumberExtractor.Matches(this.Title).FirstOrDefault()?.Captures.FirstOrDefault()?.Value?.Split()[0];
    this.EpisodeNumber = episodeNumberString == null ? (int?)null : int.Parse(episodeNumberString);

    {
      this.Duration =
        docRoot.TryGetProperty("runtime", out var element) && element.ValueKind == JsonValueKind.Number
          ? TimeSpan.FromSeconds(element.GetDouble())
          : null;
    }
    {
      this.ReleaseDate =
        docRoot.TryGetProperty("releaseDate", out var element) && element.ValueKind == JsonValueKind.Number
          ? DateTimeOffset.FromUnixTimeSeconds((long)element.GetDouble())
          : null;
    }
    {
      this.ShowDate =
        docRoot.TryGetProperty("showDate", out var element) && element.ValueKind == JsonValueKind.Number
          ? DateTimeOffset.FromUnixTimeSeconds((long)element.GetDouble())
          : null;
    }
  }

  public string Title { get; init; }
  public string Description { get; init; }
  public bool IsLostEpisode { get; init; }
  public int? EpisodeNumber { get; init; }
  public TimeSpan? Duration { get; init; }
  public DateTimeOffset? ReleaseDate { get; init; }
  public DateTimeOffset? ShowDate { get; init; }
}

record RssItem
{
  public RssItem(XElement xItem, int episodeNumber)
  {
    this.Title = xItem.Element("title")?.Value ?? "";
    this.Description = xItem.Element("description")?.Value ?? "";
    this.EpisodeNumber = episodeNumber;
    this.Duration =
      (xItem.Element("duration")?.Value != null)
        ? TimeSpan.FromSeconds(int.Parse(xItem.Element("duration")!.Value))
        : (TimeSpan?)null;
    this.ReleaseDate =
      xItem.Element("pubDate")?.Value != null
        ? DateTimeOffset.Parse(xItem.Element("pubDate")!.Value)
        : (DateTimeOffset?)null;
    this.IsTrailer = xItem.Element("episodeType")?.Value == "trailer";
    this.HasExplicitLanguage = xItem.Element("explicit")?.Value == "yes";
    this.SoundFile = xItem.Element("url")?.Value;
  }

  public string Title { get; init; }
  public string Description { get; init; }
  public int EpisodeNumber { get; init; }
  public TimeSpan? Duration { get; init; }
  public DateTimeOffset? ReleaseDate { get; init; }
  public bool IsTrailer { get; init; }
  public bool HasExplicitLanguage { get; init; }
  public string? SoundFile { get; init; }
}