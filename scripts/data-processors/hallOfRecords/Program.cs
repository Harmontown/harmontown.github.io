using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

/// Sourced by using FireFox's "inspect element" and copy/pasting the HTML element.
const string Input = "../../../data/raw/hall-of-records.xml";

const string Output = "../../../data/hall-of-records.json";

void Scrape()
{
  var doc = File.ReadAllLines(Input);
  var epNumRegex = new Regex("title=\"Episode (\\d\\d\\d)");
  var listIdRegex = new Regex("href=\"/watch.*list=(.*)\"");
  
  var entries =
    doc
      .Where(line => line.Trim().StartsWith("<a id=\"video-title\""))
      .Select(line => new Entry(
        episodeNumber: int.Parse(epNumRegex.Match(line).Groups[1].Value),
        url: $"https://www.youtube.com/playlist?list={listIdRegex.Match(line).Groups[1].Value}"
      ))
      .ToList();

  var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });

  File.WriteAllText(Output, json);
}

Scrape();

record Entry(int episodeNumber, string url);