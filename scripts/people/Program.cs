using System.Text.Json;

const string PDInput = "../../data/raw/podcastDynamite/";
const string PeopleOutput = "../../data/people.json";
const string RolesOut = "../../data/roles.json";

void AggregatePeople()
{
  const int first = 1;
  const int last = 377;

  var peopleByEpisode =
    Enumerable
      .Range(first, last)
      .ToDictionary(item => item, GetEpisodePeople);

  var people =
    peopleByEpisode.Values
      .SelectMany(item => item)
      .Select(item => new
      {
        id = item.personId,
        name = string.Join(' ',
          new[] { item.firstName, item.middleName, item.lastName }
            .Where(item => !string.IsNullOrWhiteSpace(item))).Trim()
      })
      .OrderBy(item => item.id)
      .Distinct()
      .ToList();

  var roles =
    peopleByEpisode.Values
      .SelectMany(item => item)
      .Select(item => new { id = item.roleId, name = item.name })
      .OrderBy(item => item.id)
      .Distinct()
      .ToList();
  {
    var json = JsonSerializer.Serialize(people, new JsonSerializerOptions() { WriteIndented = true });
    File.WriteAllText(PeopleOutput, json);
  }
  {
    var json = JsonSerializer.Serialize(roles, new JsonSerializerOptions() { WriteIndented = true });
    File.WriteAllText(RolesOut, json);
  }
}

IEnumerable<Person> GetEpisodePeople(int i)
{
  var doc = JsonDocument.Parse(File.ReadAllText($"{PDInput}{i:D3}/people.json"));
  var result = doc.RootElement.EnumerateArray().Select(item => item.Deserialize<Person>()!).ToList();
  return result;
}

AggregatePeople();

record Person(int roleId, string firstName, string middleName, string lastName, string name, int personId);
