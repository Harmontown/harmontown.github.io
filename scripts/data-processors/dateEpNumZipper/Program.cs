using System.Text.Json;

const string Directory = "../../data-scrapers/dotComScraper/directory.json";
const string VideoPostDates = "../../../data/video-post-dates.json";

var videoPostDates = 
  JsonDocument.Parse(File.ReadAllText(VideoPostDates))
    .RootElement
    .EnumerateArray()
    .Select(item => new Post(item.GetProperty("episodeNumber").GetInt32(), item.GetProperty("date").GetDateTimeOffset()))
    .ToList();

var zip = 
  JsonDocument.Parse(File.ReadAllText(Directory))
    .RootElement
    .EnumerateArray()
    .Select(item => DateTimeOffset.Parse(item.GetProperty("date").GetString()))
    .Select(item => item.Add(-item.TimeOfDay))
    .ToDictionary(
      key => key, 
      fileDate => videoPostDates.Where(post => 
        (fileDate - post.date) > TimeSpan.FromDays(-3) &&
        (fileDate - post.date) < TimeSpan.FromDays(1))
    );


foreach(var entry in zip)
{
  Console.WriteLine($"{entry.Key} - {entry.Value.Count()} - {entry.Value.FirstOrDefault()}");
}

record Post(int episodeNumber, DateTimeOffset date);