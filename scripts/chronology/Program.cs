using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// var externalImporter = new ExternalSourcesImporter();
// externalImporter.DeriveChronology();

const string Output = "../../data/chronology.yaml";

var serialiser = 
  new SerializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .DisableAliases()
    .Build();

var extractor = new FrontMatterExtractor(); 

var episodes =  extractor.GetEpisodes().ToList();

File.WriteAllText(Output, serialiser.Serialize(episodes));