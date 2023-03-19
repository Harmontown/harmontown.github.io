using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

/// Sourced by using FireFox's "inspect element" and copy/pasting the HTML element.
const string Input = "../../data/raw/thetvdb.xml";

const string Output = "../../data/thetvdb.json";

void Scrape()
{
  var doc = XDocument.Parse(File.ReadAllText(Input));

  var entries =
    doc.Element("ul")!.Elements("li")
      .Select(entry => new Entry(
        episodeNumber: int.Parse(entry.Descendants("span").First().Value.Substring(4).Trim()),
        date: DateTimeOffset.Parse(entry.Descendants("li").First().Value.Trim()),
        title: entry.Descendants("a").First().Value.Trim(),
        description: entry.Descendants("p").First().Value.Trim()
      ))
      .ToList();

  var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
  File.WriteAllText(Output, json);
}

Scrape();

record Entry(int episodeNumber, DateTimeOffset date, string title, string description);