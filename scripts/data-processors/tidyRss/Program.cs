using System.Xml.Linq;

const string RawInput = "../../../data/raw/feeds.megaphone.fm-harmontown.xml";
const string Output = "../../../data/rss-tidied.xml";

void ProcessRawInput()
{
  var input = File.ReadAllText(RawInput);

  input = input
    .Replace('’', '\'')
    .Replace("\nLearn more about your ad choices. Visit megaphone.fm/adchoices", "")
    ;

  var doc = XDocument.Parse(input);
  var itunesNS = doc.Root!.GetNamespaceOfPrefix("itunes");
  var contentNS = doc.Root!.GetNamespaceOfPrefix("content");

  doc.Descendants(itunesNS! + "title").ToList().ForEach(x => x.Remove());
  doc.Descendants().Where(x => x.Name.Namespace == itunesNS!).ToList().ForEach(x => x.Name = x.Name.LocalName);

  doc.Descendants("description").ToList().ForEach(x => x.Value = x.Value.Trim());
  doc.Descendants("summary").ToList().ForEach(x => x.Value = x.Value.Trim());
  doc.Descendants("title").ToList().ForEach(x => x.Value = x.Value.Trim());
  doc.Descendants("author").ToList().ForEach(x => x.Remove());
  doc.Descendants("subtitle").ToList().ForEach(x => x.Remove());
  doc.Descendants("summary").ToList().ForEach(x => x.Remove());
  doc.Descendants(contentNS! + "encoded").ToList().ForEach(x => x.Remove());

  doc.Descendants("guid").ToList().ForEach(x => x.Remove());
  doc.Descendants("enclosure").ToList().ForEach(x =>
  {
    x.Parent!.Add(new XElement("url", x.Attribute("url")!.Value));
    x.Remove();
  });

  File.WriteAllText(Output, doc.ToString());
}

ProcessRawInput();