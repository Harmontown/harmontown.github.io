using System.Text.Json;

// var externalImporter = new ExternalSourcesImporter();
// externalImporter.DeriveChronology();

const string Output = "../../data/chronology.json";

var extractor = new FrontMatterExtractor();

var episodes = extractor.GetEpisodes().ToList();

File.WriteAllText(Output, JsonSerializer.Serialize(episodes, new JsonSerializerOptions { WriteIndented = true }));