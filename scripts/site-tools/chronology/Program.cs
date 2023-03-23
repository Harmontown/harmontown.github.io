using System.Text.Json;

// var externalImporter = new ExternalSourcesImporter();
// externalImporter.DeriveChronology();

const string Output = "../../../data/chronology.json";
const string Input = "../../../data/thetvdb.json";

var episodes = new FrontMatterExtractor().GetEpisodes().OrderBy(item => item.SequenceNumber).ToList();
// episodes = MergeTheTVDB(episodes);

File.WriteAllText(Output, JsonSerializer.Serialize(episodes, new JsonSerializerOptions { WriteIndented = true }));


static List<Episode> MergeTheTVDB(List<Episode> episodes)
{
  var epsByNumber = episodes.Where(item => item.EpisodeNumber.HasValue).ToDictionary(item => item.EpisodeNumber!.Value, item => item);

  var doc = JsonDocument.Parse(File.ReadAllText(Input));
  foreach(var item in doc.RootElement.EnumerateArray())
  {
    int epNum = item.GetProperty("episodeNumber").GetInt32();
    DateTimeOffset date = item.GetProperty("date").GetDateTimeOffset();
    var title = item.GetProperty("title").GetString();
    var description = item.GetProperty("description").GetString();

    // epsByNumber[epNum] = epsByNumber[epNum] with { Description = description };
    epsByNumber[epNum] = epsByNumber[epNum] with { ShowDate = date.ToString("u") };
  }

  episodes = episodes.Select(item => !item.EpisodeNumber.HasValue ? item : epsByNumber[item.EpisodeNumber.Value]).ToList();
  return episodes;
}