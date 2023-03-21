using System.Text.Json;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

/// Sourced by using FireFox's "inspect element" and copy/pasting the HTML element.
const string Input = "../../../data/raw/reddit-wiki.xml";

const string Output = "../../../data/reddit-wiki.json";

void Scrape()
{
  var doc = XDocument.Parse(File.ReadAllText(Input));

  var entries =
    doc.Element("table")!.Element("tbody")!.Descendants("tr")
      .Select(row => row.Elements("td").ToList())
      .Select(tds =>
      {
        if (!int.TryParse(tds?[0].Element("a")!.Value, out var epNum)) {
          return null;
        }
        var entry = new Entry(
          episodeNumber:  epNum,
          title: tds[1].Element("a")!.Value.Replace("\n          ", " "),
          description: tds[2].Value.Replace("\n        ","")
        );
        return entry;
      })
      .Where(item => item != null && item.episodeNumber != -1)
      .ToList();

  var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
  File.WriteAllText(Output, json);
}

Scrape();

record Entry(int episodeNumber, string title, string description);