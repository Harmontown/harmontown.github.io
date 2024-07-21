using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

DateTimeOffset First = new DateTimeOffset(2011, 01, 01, 0, 0, 0, TimeSpan.Zero);
DateTimeOffset Last = new DateTimeOffset(2019, 12, 04, 0, 0, 0, TimeSpan.Zero);

DateTimeOffset Start = new DateTimeOffset(2017, 01, 01, 0, 0, 0, TimeSpan.Zero);

// CreateEmptyScan();
// await Scan();
// await CreateDirectory();

await DownloadDirectory();

async Task DownloadDirectory()
{


  var directory = LoadDirectory();
  // var client = new HttpClient();

  var items =
    directory
      .SelectMany(item => item.downloads)
      .Where(item => item != null)
      .Cast<Download>()
      .ToList();

  var completedCount = items.Where(item => item.IsCompleted()).Count();
  var partialCount = items.Where(item => item.IsPartial()).Count();
  var todoCount = items.Where(item => !item.IsPartial() && !item.IsCompleted()).Count();

  Console.WriteLine($"  Completed: {completedCount.ToString().PadLeft(3)}");
  Console.WriteLine($"    Partial: {partialCount.ToString().PadLeft(3)}");
  Console.WriteLine($"      To Do: {todoCount.ToString().PadLeft(3)}");
  Console.WriteLine($"  --------------");
  Console.WriteLine($"      Total: {items.Count.ToString().PadLeft(3)}");

  var remaining =
    items
      .Where(item => !item.IsCompleted())
      .OrderByDescending(d => d.size)
      .ToList();

  int i = 0;
  foreach (var item in remaining)
  {
    var filename = item.GetFilename();
    var targetFile = new FileInfo(Constants.OutDir + filename);

    bool skip = false;
    if (targetFile.Exists)
    {
      if (targetFile.Length == item.size)
      {
        Console.WriteLine($" -i- Matching file for {filename} found.  Skipping download.");
        skip = true;
      }

      if (targetFile.Length < item.size)
      {
        Console.WriteLine($" -!- Incomplete file for {filename} found.  Resuming download.");
      }

      if (targetFile.Length > item.size)
      {
        Console.WriteLine($" =!= Larger file for {filename} found.  wtf?.");
        throw new InvalidOperationException();
      }
    }

    if (!skip)
    {
      await DownloadFile(item, filename, targetFile, i + 1, remaining.Count);
      await Task.Delay(TimeSpan.FromSeconds(2));
    }

    // TakeScreenshots(SnapsDir, TempDir, filename, targetFile);
    i++;
  }

  static async Task DownloadFile(Download item, string filename, FileInfo targetFile, int nth, int total)
  {
    Console.WriteLine($"[ {nth:D3} of {total:D3} ] Downloading {filename} ( {Math.Ceiling(item.size / 1024.0):n0} KB ) ...");

    await Process.Start("curl", $"{item.url} --progress-bar -C - --output {targetFile.FullName}").WaitForExitAsync();
    // using (var stream = await client.GetStreamAsync(item.url))
    // {
    //   using (var fileStream = targetFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
    //   {
    //     await stream.CopyToAsync(fileStream);
    //   }
    // }

    Console.WriteLine($" Done.");
  }

  static void TakeScreenshots(string SnapsDir, string TempDir, string filename, FileInfo targetFile)
  {
    /*
    * TODO:  Limit each episode to 1-2 MB of screenshots.  You will need to estimate this according to resolution.
    * At some point, the video resolution increases dramatically, making the screenshots rather large.
    * Find the sweet spot for screenshot size and count for each episode.
    */


    Console.Write($"Taking screenshots of {filename}...");

    var epSnaps = new DirectoryInfo(SnapsDir + filename + "/");
    if (epSnaps.Exists)
    {
      Console.WriteLine(" Nevermind, they've already been taken.");
      return;
    }

    var tmp = new DirectoryInfo(TempDir);
    if (tmp.Exists)
    {
      tmp.Delete(recursive: true);
    }
    tmp.Create();


    var first =
      new ProcessStartInfo(
        "c:\\Program Files\\ffmpeg\\bin\\ffmpeg.exe",
        $"-y -loglevel quiet -i {targetFile.FullName} -ss 00:00:45 -frames:v 1 {tmp.FullName}\\00.png");
    var subsequent =
      new ProcessStartInfo(
        "c:\\Program Files\\ffmpeg\\bin\\ffmpeg.exe",
        $"-y -loglevel quiet -i {targetFile.FullName} -vf fps=1/360 {tmp.FullName}\\%02d.png");

    Process.Start(first)!.WaitForExit();
    Process.Start(subsequent)!.WaitForExit();

    if (tmp.GetFiles().Any())
    {
      tmp.MoveTo(epSnaps.FullName);
    }

    Console.WriteLine(" Done.");
  }
}

void PrintDirectorySize()
{
  var directory = LoadDirectory();
  var totalSize = directory.SelectMany(item => item.downloads.Select(d => d.size)).Sum();
  Console.WriteLine($"Directory Total Size: {totalSize}");
}

async Task CreateDirectory()
{
  var hits = LoadScanHits();
  var directory = LoadDirectory();

  var client = new HttpClient();

  for (int i = 0; i < hits.Count; i++)
  {
    var hit = hits[i];
    var item = directory[i];

    if (item.IsPopulated())
    {
      continue;
    }

    var urlBase = $"{Constants.UrlBase}{item.date.Year}-{item.date.Month:D2}-{item.date.Day:D2}-";
    if (hit.hasFinal!.Value)
    {
      var url = $"{urlBase}final.mp4";
      item.downloads.Add(new Download(url, await GetSize(url)));
    }
    if (hit.hasLow!.Value)
    {
      var url = $"{urlBase}low.mp4";
      item.downloads.Add(new Download(url, await GetSize(url)));
    }
    if (hit.hasH265!.Value)
    {
      var url = $"{urlBase}h265.mp4";
      item.downloads.Add(new Download(url, await GetSize(url)));
    }
    if (hit.hasAudio!.Value)
    {
      var url = $"{urlBase}audio.mp3";
      item.downloads.Add(new Download(url, await GetSize(url)));
    }

    SaveDirectory(directory);
  }

  async Task<long> GetSize(string url)
  {
    var request = new HttpRequestMessage(HttpMethod.Head, url);
    request.Headers.Host = "download.harmontown.com";
    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("insomnia", "2023.1.0"));
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
    var response = await client!.SendAsync(request);
    await Task.Delay(TimeSpan.FromSeconds(0.125));

    // Console.WriteLine($"Sent: {request.ToString()}");
    // Console.WriteLine($"Received {response.ToString()}");
    // foreach(var header in response.Content.Headers)
    // {
    //   Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
    // }

    return long.Parse(response.Content.Headers.Single(item => item.Key == "Content-Length").Value.Single());
  }
}

async Task Scan()
{

  var scan = LoadScan();

  var client = new HttpClient();

  int i = scan.Count - 1;
  while (i >= 0)
  {
    var item = scan[i];
    if (item.IsChecked())
    {
      // i -= item.IsHit() ? 7 : 1;
      i--;
      Console.Write(item.IsHit() ? "!" : "o");
      continue;
    }

    var urlBase = $"{Constants.UrlBase}{item.date.Year}-{item.date.Month:D2}-{item.date.Day:D2}-";
    var hasFinal = await Exists(item.hasFinal, $"{urlBase}final.mp4");
    var hasLow = await Exists(item.hasLow, $"{urlBase}low.mp4");
    var hasH265 = await Exists(item.hasH265, $"{urlBase}h265.mp4");
    var hasAudio = await Exists(item.hasAudio, $"{urlBase}audio.mp3");

    var isSuccess = hasFinal || hasLow || hasAudio;
    scan[i] = item with { hasFinal = hasFinal, hasLow = hasLow, hasH265 = hasH265, hasAudio = hasAudio };
    SaveScan(scan);

    if (isSuccess)
    {
      Console.WriteLine($"Found stuff for {item.date.ToString("u").Split()[0]}");
      // i -= 7;
      i--;
    }
    else
    {
      Console.Write($".");
      i--;
    }
  }

  async Task<bool> Exists(bool? current, string url)
  {
    if (current.HasValue)
    {
      return current.Value;
    }
    var result = (await client!.SendAsync(new HttpRequestMessage(HttpMethod.Head, url))).StatusCode == System.Net.HttpStatusCode.OK;
    await Task.Delay(TimeSpan.FromSeconds(0.5));
    return result;
  }
}

List<ScanEntry> LoadScan()
{
  var scan = JsonSerializer.Deserialize<ScanEntry[]>(File.ReadAllText(Constants.ScanFile))!.ToList();

  Console.WriteLine($"Scan with {scan.Count} entries loaded.");

  return scan;
}

List<ScanEntry> LoadScanHits()
  => LoadScan().Where(item => item.IsHit()).ToList();

List<DirectoryEntry> LoadDirectory()
{
  if (!File.Exists(Constants.DirectoryFile))
  {
    var hits = LoadScanHits();
    return hits.Select(item => new DirectoryEntry(item.date, new List<Download>())).ToList();
  }
  return JsonSerializer.Deserialize<DirectoryEntry[]>(File.ReadAllText(Constants.DirectoryFile))!.ToList();
}

void CreateEmptyScan()
{
  var scan =
    Enumerable
      .Range(0, (int)Math.Ceiling((Last - First).TotalDays))
      .Select(item => new ScanEntry(First.AddDays(item)))
      .ToList();

  Console.WriteLine($"Scan with {scan.Count} entries created.");
  SaveScan(scan);
}

void SaveScan(List<ScanEntry> scan)
  => File.WriteAllText(Constants.ScanFile, JsonSerializer.Serialize(scan, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));

void SaveDirectory(List<DirectoryEntry> directory)
  => File.WriteAllText(Constants.DirectoryFile, JsonSerializer.Serialize(directory, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));

public static class Constants
{
  // TODO: uncomment and fix the paths for the next two lines.
  // public const string ScanFile = "./scan.json";
  // public const string DirectoryFile = "./directory.json";
  public const string UrlBase = "https://download.harmontown.com/video/harmontown-";
  public const string OutDir = "E:/Harmontown/";
  const string SnapsDir = OutDir + "snaps/";
  const string TempDir = SnapsDir + "tmp/";
}

record ScanEntry(DateTimeOffset date, bool? hasFinal = null, bool? hasLow = null, bool? hasH265 = null, bool? hasAudio = null)
{
  public bool IsChecked() => this.hasFinal.HasValue && this.hasLow.HasValue && this.hasH265.HasValue && this.hasAudio.HasValue;
  public bool IsHit() => this.IsChecked() && (this.hasFinal!.Value || this.hasLow!.Value || this.hasH265!.Value || this.hasAudio!.Value);
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