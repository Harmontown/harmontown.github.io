using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

const string PDInput = "../../data/raw/podcastDynamite/";
const string RssInput = "../../data/rss-tidied.xml";
const string PeopleInput = "../../data/people.json";
const string RolesInput = "../../data/roles.json";

const string Output = "../../data/chronology.json";
const int PDLast = 377;
const int rssLast = 364;

const int ComprollerRoleId = 3;
const int GuestRoleId = 2;
const int AudienceGuestRoleId = 5;
DateTimeOffset MeltDownStart = new DateTimeOffset(2012, 1, 1, 0, 0, 0, TimeSpan.Zero);

void DeriveChronology()
{
  var peopleById =
    JsonSerializer.Deserialize<IdNamePair[]>(File.ReadAllText(PeopleInput))!
      .ToDictionary(item => item.id, item => item.name);
  var rolesById =
    JsonSerializer.Deserialize<IdNamePair[]>(File.ReadAllText(RolesInput))!
      .ToDictionary(item => item.id, item => item.name);

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
      var episode = JsonDocument.Parse(File.ReadAllText($"{PDInput}{pdId:D3}/episode.json"));
      var episodePeopleIdsByRoleId =
        JsonDocument.Parse(File.ReadAllText($"{PDInput}{pdId:D3}/people.json"))
          .RootElement.EnumerateArray()
          .Select(item => (personId: item.GetProperty("personId").GetInt32(), roleId: item.GetProperty("roleId").GetInt32()))
          .ToLookup(item => item.roleId, item => item.personId);

      var comptrollerId = episodePeopleIdsByRoleId[ComprollerRoleId].FirstOrDefault();
      var guestIds = episodePeopleIdsByRoleId[GuestRoleId].ToArray();
      var audienceGuestIds = episodePeopleIdsByRoleId[AudienceGuestRoleId].ToArray();

      peopleById.TryGetValue(comptrollerId, out var pdComptroller);
      var pdGuests =
        guestIds
          .Select(id => peopleById.TryGetValue(id, out var name) ? name : null)
          .Where(item => !string.IsNullOrWhiteSpace(item))
          .Cast<string>()
          .ToArray();
      var pdAudienceGuests =
        audienceGuestIds
          .Select(id => peopleById.TryGetValue(id, out var name) ? name : null)
          .Where(item => !string.IsNullOrWhiteSpace(item))
          .Cast<string>()
          .ToArray();

      pdItem = new PDItem(episode.RootElement, pdComptroller ?? "", pdGuests, pdAudienceGuests);
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

    var title = rssItem?.Title ?? pdItem?.Title ?? "";
    var comptroller = pdItem?.Comptroller ?? "";
    var guests = (pdItem?.Guests ?? new string[0]).ToList();
    var audienceGuests = (pdItem?.AudienceGuests ?? new string[0]).ToList();
    var showDate = pdItem?.ShowDate;

    WrangleMoreValues(
      sequenceNumber,
      comptroller,
      guests,
      audienceGuests,
      ref title,
      ref showDate,
      out var venue);

    var entry = new
    {
      sequenceNumber = sequenceNumber,
      pdId = isPDOver ? (int?)null : pdId,
      rssIndex = rssItem == null ? (int?)null : rssIndex,
      episodeNumber = (pdItem?.IsLostEpisode ?? false) ? (int?)null : episodeNumber,
      isLostEpisode = pdItem?.IsLostEpisode ?? false,
      isTrailer = rssItem?.IsTrailer ?? false,
      title = title,
      venue = venue,
      comptroller = comptroller,
      gameMaster = (string?)null,
      hasDandD = (bool?)null,
      guests = guests,
      audienceGuests = audienceGuests,
      image = "episode-placeholder.jpg",
      description = rssItem?.Description ?? pdItem?.Description ?? "",
      showDate = showDate,
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

void WrangleMoreValues(
  int sequenceNumber,
  string comptroller,
  List<string> guests,
  List<string> audienceGuests,
  ref string title,
  ref DateTimeOffset? showDate,
  out string venue
  )
{
  /// Last minute data wrangling:
  // infer showDate (not sure this is used)
  {
    var showDateFromTitleRegex = new Regex("\\((\\d{1,2}\\.\\d{1,2}\\.\\d{2,4})\\)$");
    var match = showDateFromTitleRegex.Match(title);
    if (match.Success)
    {
      title = title.Substring(0, title.Length - match.Length).Trim();
      if (showDate == null)
      {
        var dateParts = match.Groups[1].Value.Split(".").Select(item => int.Parse(item)).ToArray();
        var year = (dateParts[2] % 2000) + 2000;
        var month = dateParts[0];
        var day = dateParts[1];
        showDate = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
      }
    }
  }

  // assume venue
  {
    venue = sequenceNumber switch
    {
      <= 36 => "NerdMelt",
      37 => "TBC, Phoenix, AZ",
      >= 38 and < 56 => $"TBC, {title.Split(":")[1].Trim()}",
      56 => "Egyptian Theatre, Los Angeles, CA",
      >= 57 and <= 78 => "NerdMelt",
      _ => "TBC"
    };
  }

  // extract guest from title
  {
    var titleGuestExtractor = new Regex("\\((\\D+)\\)$");
    var match = titleGuestExtractor.Match(title);
    if (match.Success)
    {
      title = title.Substring(0, title.Length - match.Captures[0].Value.Length).Trim();
      var titleGuests =
         match.Groups[1].Value
          .Split(",")
          .Select(item => item.Trim())
          .ToList();

      titleGuests =
        titleGuests
          .Except(guests)
          .Except(audienceGuests)
          .Where(item => item != comptroller)
          .ToList();

      guests.AddRange(titleGuests);
    }
  }
}

DeriveChronology();

record PDItem
{
  public PDItem(JsonElement docRoot, string comptroller, string[] guests, string[] audienceGuests)
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
      if (this.Duration == TimeSpan.Zero)
      {
        this.Duration = null;
      }
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

    this.Comptroller = comptroller;
    this.Guests = guests;
    this.AudienceGuests = audienceGuests;
  }

  public string Title { get; init; }
  public string Description { get; init; }
  public bool IsLostEpisode { get; init; }
  public int? EpisodeNumber { get; init; }
  public string Comptroller { get; init; }
  public string[] Guests { get; init; }
  public string[] AudienceGuests { get; init; }
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

record IdNamePair(int id, string name);