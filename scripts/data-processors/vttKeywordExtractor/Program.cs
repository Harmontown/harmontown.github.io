using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

const string Output = "../../../data/episodeVttKeywords.json";

// var episodeWordTallies = JsonSerializer.Deserialize<List<Dictionary<string, int>>>(File.ReadAllText("./episodeWordTallies.json"));
// var globalWordCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText("./globalWordCounts.json"));
var episodeWordTallies = GetEpisodeWordTallies();
// File.WriteAllText("./episodeWordTallies.json", JsonSerializer.Serialize(episodeWordTallies, new JsonSerializerOptions { WriteIndented = true }));
var globalWordCounts = GetGlobalWordCounts(episodeWordTallies);
// File.WriteAllText("./globalWordCounts.json", JsonSerializer.Serialize(globalWordCounts, new JsonSerializerOptions { WriteIndented = true }));

var output = new List<Entry>();

for (int i = 0; i < episodeWordTallies.Count; i++)
{
  var epNum = i + 1;
  var epWords = episodeWordTallies[i];

  int minOccurences = 3;

  var wordScores =
    epWords
      .Where(kvp => kvp.Value >= minOccurences)
      .Select(kvp => kvp.Key)
      .ToDictionary(
        key => key,
        word => (double)globalWordCounts[word] / epWords[word]);

  var keywords =
    wordScores
      .OrderBy(kvp => kvp.Value).ThenBy(kvp => kvp.Key)
      .Where(kvp => kvp.Value < (minOccurences * 2.5))
      .Select(kvp => kvp.Key)
      .Take(25)
      .ToList();

  const string vttZipFilenameFormat = "/assets/transcripts/Harmontown.S01E{0:D3}.vtt.zip";

  output.Add(new Entry(epNum, string.Format(vttZipFilenameFormat, epNum), keywords));
}

File.WriteAllText(Output, JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
// output.ForEach(line => Console.WriteLine(line));

static Dictionary<string, int> GetGlobalWordCounts(List<Dictionary<string, int>> episodeWordTallies)
{
  var globalWordCounts =
    episodeWordTallies
      .SelectMany(epDict => epDict.Keys)
      .Distinct()
      .ToDictionary(key => key, value => 0);

  foreach (var epDict in episodeWordTallies)
  {
    foreach (var entry in epDict)
    {
      globalWordCounts[entry.Key] += entry.Value;
    }
  }

  return new Dictionary<string, int>(globalWordCounts.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key));
}

static List<Dictionary<string, int>> GetEpisodeWordTallies()
{
  const string InputFormat = "../../../data/raw/harmondev.duckdns.org/Harmontown.S01E{0:D3}.vtt";

  return Enumerable.Range(1, 361)
      .Select(i => string.Join(
        " ",
        File.ReadLines(string.Format(InputFormat, i)).Skip(1)
          .Chunk(3)
          .Select(chunk => chunk.Skip(2).FirstOrDefault()?.Trim())
          .Where(item => item is not null))
        .ToLowerInvariant()
        .Split(' ', '.', '?', '!', ',', '…', '[', ']', '"')
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .GroupBy(item => item)
        .ToDictionary(group => group.Key, group => group.Count()))
      .Select(dict => new Dictionary<string, int>(dict.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key)))
      .ToList();
}

record Entry(int EpNum, string VttZipFilename, List<string> Keywords);