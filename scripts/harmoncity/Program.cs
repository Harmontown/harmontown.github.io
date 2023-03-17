using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

const string Output = "../../data/raw/harmoncity.json";

async Task Scrape()
{
  int[] episodes = {
    1,6,12,14,17,18,19,28,29,35,38,44,45,48,50,70,71,77,79,81,83,91,105,112,151,158,189,190,194,195,198,
    200,202,205,207,216,217,222,239,240,241,243,244,245,246,247,249,253,258,259,260,261,263,266,267,268,
    270,271,272,273,275,276,277,278,279,280,281,282,283,286,287,288,289,290,291,295,344
  };
  const string EndpointPrefix = "https://harmon.city/episode-";

  var client = new HttpClient();
  var regex = new Regex("\\<p\\>(.+)\\</p\\>");

  var result = new Dictionary<int, string>();
  foreach (var epNum in episodes)
  {
    result[epNum] = await GetDescription(epNum);
  }
  Dump(Output, result);

  async Task<string> GetDescription(int episodeNumber)
    => regex.Matches(await Get(EndpointPrefix + episodeNumber))[1].Groups[1].Value;

  async Task<string> Get(string url)
    => await (await client.GetAsync((await client.GetAsync(url)).Headers.Location)).Content.ReadAsStringAsync();

  void Dump<T>(string outFile, T obj)
  {
    var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(outFile, json);
  }
}

await Scrape();