using System.Text.Json;

const string UrlFormat = "https://harmondev.duckdns.org/api/vtt/stream/Harmontown.S01E{0:D3}.vtt";
const string FileFormat = "../../../data/raw/harmondev.duckdns.org/Harmontown.S01E{0:D3}.vtt";

// await Scrape();

async Task Scrape()
{
  var client = new HttpClient();
  for (int i = 1; i <= 361; i++)
  {
    var url = string.Format(UrlFormat, i);
    var filename = string.Format(FileFormat, i);

    var targetFile = new FileInfo(filename);
    using (var stream = await client.GetStreamAsync(url))
    {
      var response = await JsonDocument.ParseAsync(stream);
      File.WriteAllText(targetFile.FullName, response.RootElement.GetProperty("content").GetString());
    }
  }
}