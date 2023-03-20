using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

var directory = LoadDirectory();

var tvdb =
  JsonDocument.Parse(File.ReadAllText("../../data/thetvdb.json"))
    .RootElement
    .EnumerateArray()
    .Select(item => (epNum: item.GetProperty("episodeNumber").GetInt32(), date: item.GetProperty("date").GetDateTimeOffset()))
    .ToList();

var seqNumByEpNum =
  JsonDocument.Parse(File.ReadAllText("../../data/chronology.json"))
    .RootElement
    .EnumerateArray()
    .Where(item => item.GetProperty("EpisodeNumber").ValueKind != JsonValueKind.Null)
    .ToDictionary(key => key.GetProperty("EpisodeNumber").GetInt32(), value => value.GetProperty("SequenceNumber").GetInt32());


// ScreenshotAllEpisodes();
// RenameToSequenceNumber();

void RenameToSequenceNumber()
{
  var episodeNumbers = 
    directory
      .Select(entry => tvdb.Single(item => item.date.Date == entry.date.Date).epNum)
      .OrderDescending()
      .ToList();

  foreach (var episodeNumber in episodeNumbers)
  {
    var sequenceNumber = seqNumByEpNum[episodeNumber];

    var oldName = Constants.SnapsDir + $"{episodeNumber:D3}";
    var newName = Constants.SnapsDir + $"{sequenceNumber:D3}";

    var folder = new DirectoryInfo(oldName);
    folder.MoveTo(newName);
  }
}

void ScreenshotAllEpisodes()
{
  foreach (var entry in directory)
  {
    var match = tvdb.Single(item => item.date.Date == entry.date.Date);
    var epNum = match.epNum;

    var download = entry.downloads.Where(item => item.url.EndsWith(".mp4")).OrderByDescending(item => item.size).First();
    var sourceFile = new FileInfo(Constants.OutDir + download.GetFilename());

    TakeScreenshots(sourceFile, Constants.SnapsDir + $"{epNum:D3}");
  }
}

static void TakeScreenshots(FileInfo sourceFile, string destination)
{
  Console.Write($"Taking screenshots of {sourceFile.Name} into {destination}...");

  var epSnaps = new DirectoryInfo(destination);
  if (epSnaps.Exists)
  {
    Console.WriteLine(" Nevermind, they've already been taken.");
    return;
  }

  var tmp = new DirectoryInfo(Constants.TempDir);
  if (tmp.Exists)
  {
    tmp.Delete(recursive: true);
  }
  tmp.Create();

  var first =
    new ProcessStartInfo(
      "c:\\Program Files\\ffmpeg\\bin\\ffmpeg.exe",
      $"-y -loglevel quiet -i {sourceFile.FullName} -s 768x432 -ss 00:00:45 -frames:v 1 {tmp.FullName}\\00.png");
  var subsequent =
    new ProcessStartInfo(
      "c:\\Program Files\\ffmpeg\\bin\\ffmpeg.exe",
      $"-y -loglevel quiet -i {sourceFile.FullName} -vf \"fps=1/1440,scale=768:432\" {tmp.FullName}\\%02d.png");

  Process.Start(first)!.WaitForExit();
  Process.Start(subsequent)!.WaitForExit();

  if (tmp.GetFiles().Any())
  {
    tmp.MoveTo(epSnaps.FullName);
  }

  Console.WriteLine(" Done.");
}

List<DirectoryEntry> LoadDirectory()
  => JsonSerializer.Deserialize<DirectoryEntry[]>(File.ReadAllText(Constants.DirectoryFile))!.ToList();

static class Constants
{
  public const string DirectoryFile = "../dotComScraper/directory.json";
  public const string UrlBase = "https://download.harmontown.com/video/harmontown-";
  public const string OutDir = "E:/Harmontown/";
  public const string SnapsDir = Constants.OutDir + "snaps/";
  public const string TempDir = SnapsDir + "tmp/";
}

record DirectoryEntry(DateTimeOffset date, List<Download> downloads)
{
  public bool IsPopulated() => this.downloads?.Count > 0;
}

record Download(string url, long size)
{
  public string GetFilename() => this.url.Split("/").Last();

  public bool IsCompleted()
  {
    var targetFile = new FileInfo(Constants.OutDir + this.GetFilename());
    return targetFile.Exists && targetFile.Length == this.size;
  }

  public bool IsPartial()
  {
    var targetFile = new FileInfo(Constants.OutDir + this.GetFilename());
    return targetFile.Exists && targetFile.Length < this.size;
  }
}