using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Web;

const string SiteMap = "../../../data/raw/harmontown.com/wp-sitemap-posts-post-1.xml";
const string Output = "../../../data/video-post-dates.json";

var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");

var doc = XDocument.Parse(File.ReadAllText(SiteMap));
var ns = doc.Root.GetDefaultNamespace().NamespaceName;

var entries =
  doc.Root
    .Descendants(XName.Get("loc", ns))
    .Select(item => item.Value.Trim())
    .Where(item => item.Split('/', StringSplitOptions.RemoveEmptyEntries)[4].StartsWith("video-"))
    .Select(async url => await ParsePage(url)).Select(item => item.Result)
    .ToList();

File.WriteAllText(Output, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));

async Task<Entry> ParsePage(string url)
{
  var episodeNumber = int.Parse(url.Split("-")[2].Substring(0, 3));
  var localCache = $"./cache/{episodeNumber}.html";
  string body;

  if (File.Exists(localCache))
  {
    body = File.ReadAllText(localCache);
  }
  else
  {
    var response = await client.GetAsync(url);
    body = await response.Content.ReadAsStringAsync();
    File.WriteAllText(localCache, body);
  }

  var title = new Regex("\\<h1 class=\"entry-title\"\\>(FREE )?Video Episode:? \\d\\d\\d:?\\s?(&#8211;)? (.+)\\</h1\\>").Match(body).Groups[3].Value;
  title = HttpUtility.HtmlDecode(title);
  var time = DateTimeOffset.Parse(new Regex("\\<time class=\"entry-date\" datetime=\"(.+)\">.+\\</time\\>").Match(body).Groups[1].Value);

  return new Entry(episodeNumber, time, title, url);
}

record Entry(int episodeNumber, DateTimeOffset date, string title, string url);