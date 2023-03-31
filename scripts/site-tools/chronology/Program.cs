using System.Text.Json;

// var externalImporter = new ExternalSourcesImporter();
// externalImporter.DeriveChronology();

const string Output = "../../../data/chronology.json";
const string TheTvDBInput = "../../../data/thetvdb.json";
const string VttInfoInput = "../../../data/episodeVttKeywords.json";

var episodes = new FrontMatterExtractor().GetEpisodes().OrderBy(item => item.SequenceNumber).ToList();
// episodes = MergeTheTVDB(episodes);
// episodes = MergeVttInfo(episodes);

File.WriteAllText(Output, JsonSerializer.Serialize(episodes, new JsonSerializerOptions { WriteIndented = true }));


static List<Episode> MergeTheTVDB(List<Episode> episodes)
{
  var epsByNumber = episodes.Where(item => item.EpisodeNumber.HasValue).ToDictionary(item => item.EpisodeNumber!.Value, item => item);

  var doc = JsonDocument.Parse(File.ReadAllText(TheTvDBInput));
  foreach (var item in doc.RootElement.EnumerateArray())
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

static List<Episode> MergeVttInfo(List<Episode> episodes)
{
  var epsByNumber = episodes.Where(item => item.EpisodeNumber.HasValue).ToDictionary(item => item.EpisodeNumber!.Value, item => item);

  var doc = JsonDocument.Parse(File.ReadAllText(VttInfoInput));
  foreach (var item in doc.RootElement.EnumerateArray())
  {
    int epNum = item.GetProperty("EpNum").GetInt32();
    var vttZipFilename = item.GetProperty("VttZipFilename").GetString();
    var keywords = item.GetProperty("Keywords").EnumerateArray().Select(item => item.GetString()!).ToArray();

    epsByNumber[epNum] = epsByNumber[epNum] with
    {
      External = epsByNumber[epNum].External with
      {
        Transcription = new TranscriptionEntry()
        {
          VttZipFilename = vttZipFilename,
          Keywords = string.Join(", ", keywords)
        }
      }
    };
  }

  episodes = episodes.Select(item => !item.EpisodeNumber.HasValue ? item : epsByNumber[item.EpisodeNumber.Value]).ToList();
  return episodes;
}