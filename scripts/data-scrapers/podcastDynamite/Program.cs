using System.Net.Http;
using System.Text.Json;

const string Output = "../../../data/raw/podcastDynamite/";

async Task Scrape()
{
  const int first = 1;
  const int last = 377;
  const string episodeEndpoint = "https://podcastdynamite.com/PodcastDynamite/api/podcasts/1/episodes/";
  const string peopleEndpoint = "https://podcastdynamite.com/PodcastDynamite/api/roles/1/";
  const string minutesEndpoint = "https://podcastdynamite.com/PodcastDynamite/api/minutes/";

  var client = new HttpClient();
  for (int i = first; i <= last; i++)
  {
    var episode = await Get(episodeEndpoint);
    var people = await Get(peopleEndpoint);
    var minutes = await Get(minutesEndpoint);

    Directory.CreateDirectory($"{Output}/{i:D3}");
    Dump($"{Output}{i:D3}/episode.json", episode);
    Dump($"{Output}{i:D3}/minutes.json", minutes);
    Dump($"{Output}{i:D3}/people.json", people);

    await Task.Delay(TimeSpan.FromSeconds(2));

    async Task<string> Get(string endpoint) => await (await client.GetAsync($"{endpoint}{i}")).Content.ReadAsStringAsync();
    void Dump(string outFile, string json)
    {
      json = JsonSerializer.Serialize(JsonDocument.Parse(json), new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(outFile, json);
    }
  }
}

await Scrape();