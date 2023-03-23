using System.Text.Json;

const string Directory = "../../data-scrapers/dotComScraper/directory.json";
const string TvDb = "../../../data/thetvdb.json";

var videoPostDates = 
  JsonDocument.Parse(File.ReadAllText(TvDb))
    .RootElement
    .EnumerateArray()
    .Select(item => new TvDbEntry(item.GetProperty("episodeNumber").GetInt32(), item.GetProperty("date").GetDateTimeOffset()))
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
        (fileDate - post.date) > TimeSpan.FromDays(-1) &&
        (fileDate - post.date) < TimeSpan.FromDays(1))
    );


foreach(var entry in zip)
{
  Console.WriteLine($"{entry.Key:u} - {entry.Value.Count()} - {entry.Value.FirstOrDefault()} - delta: {(entry.Key - entry.Value.FirstOrDefault()?.date)}");
}

record TvDbEntry(int episodeNumber, DateTimeOffset date)
{
  public override string ToString()
    => $"TvDbEntry {{ {episodeNumber} - {date:u} }}";
}