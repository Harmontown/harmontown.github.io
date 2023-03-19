using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FrontMatterExtractor
{
  const string Input = "../../docs/_episodes/";

  public IEnumerable<Episode> GetEpisodes()
  {
    var epDirs = new DirectoryInfo(Input).GetDirectories();
    var deserialiser =
      new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    foreach (var dir in epDirs)
    {
      var doc = new FileInfo(dir.FullName + "/" + "index.md");
      var lines = File.ReadAllLines(doc.FullName);
      var frontMatter =
        string.Join(
          Environment.NewLine,
          lines
            .SkipWhile(line => line.TrimEnd() != "---")
            .Skip(1)
            .TakeWhile(line => line.TrimEnd() != "---")
            .Where(line => !string.IsNullOrWhiteSpace(line)));

      Episode result;
      try
      {
        result = deserialiser.Deserialize<Episode>(frontMatter);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.InnerException?.ToString());
        throw;
      }
      yield return result;
    }
  }
}

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