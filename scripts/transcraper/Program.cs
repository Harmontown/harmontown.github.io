using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

const string UrlFormat = "https://harmondev.duckdns.org/api/vtt/stream/Harmontown.S01E{0:D3}.vtt";
const string FileFormat = "../../data/raw/harmondev.duckdns.org/Harmontown.S01E{0:D3}.vtt";

var client = new HttpClient();
for (int i = 1; i <= 361; i++)
{
  var url = string.Format(UrlFormat, i);
  var filename = string.Format(FileFormat, i);

  var targetFile = new FileInfo(filename);
  using (var stream = await client.GetStreamAsync(url))
  {
    using (var fileStream = targetFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
    {
      await stream.CopyToAsync(fileStream);
      await Task.Delay(TimeSpan.FromSeconds(1));
    }
  }
}