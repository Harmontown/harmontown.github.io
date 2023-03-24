using System.Text.Json;
using System.Text;

const string DirectoryFile = "../../data-scrapers/dotComScraper/directory.json";
const string SourceDir = "E:/Harmontown/";
const string OutFile = "E:/Harmontown.xspf";

var tvdb =
  JsonDocument.Parse(File.ReadAllText("../../../data/thetvdb.json"))
    .RootElement
    .EnumerateArray()
    .Select(item => (epNum: item.GetProperty("episodeNumber").GetInt32(), date: item.GetProperty("date").GetDateTimeOffset()))
    .ToList();

var directory = LoadDirectory();

var builder = new StringBuilder();
builder.AppendLine("""
<?xml version="1.0" encoding="UTF-8"?>
<playlist xmlns="http://xspf.org/ns/0/" xmlns:vlc="http://www.videolan.org/vlc/playlist/ns/0/" version="1">
	<title>Playlist</title>
	<trackList>
""");

int i = 0;
foreach (var entry in directory)
{
  var download = entry.downloads.Where(item => item.url.EndsWith(".mp4")).OrderByDescending(item => item.size).First();

  var match = tvdb.Single(item => item.date.Date == entry.date.Date);

  var sourceFile = new FileInfo(SourceDir + download.GetFilename());

  builder.AppendLine($"""
  		<track>
  			<title>{$"{match.epNum} - {sourceFile.Name}"}</title>
  			<location>file:///{sourceFile.FullName}</location>
  			<extension application="http://www.videolan.org/vlc/playlist/0">
  				<vlc:id>{i++}</vlc:id>
  			</extension>
  		</track>
  """);
}

builder.AppendLine("""
	</trackList>
</playlist>
""");

File.WriteAllText(OutFile, builder.ToString());

List<DirectoryEntry> LoadDirectory()
  => JsonSerializer.Deserialize<DirectoryEntry[]>(File.ReadAllText(DirectoryFile))!.ToList();

record DirectoryEntry(DateTimeOffset date, List<Download> downloads)
{
  public bool IsPopulated() => this.downloads?.Count > 0;
}

record Download(string url, long size)
{
  public string GetFilename() => this.url.Split("/").Last();
}