using System.Text.Json;
using GitHubScraper;

var config = Config.LoadConfiguration();

if (args.Length > 0)
{
  config.NumDays = int.Parse(args[0]);
}
if (args.Length > 1)
{
  config.DataFilePath = args[1];
}
// GitHub GraphQL API endpoint
string apiUrl = config.ApiUrl;

// Your GitHub personal access token
string accessToken = config.AccessToken;

// GraphQL query to fetch data from the calendar API
string query = $@"
query {{
  viewer {{
    contributionsCollection(from: ""{DateTime.Now.AddDays(1-config.NumDays):yyyy-MM-ddTHH:mm:ssZ}"", to: ""{DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}"") {{
      contributionCalendar {{
        totalContributions
        weeks {{
          contributionDays {{
            contributionCount
            date
          }}
        }}
      }}
    }}
  }}
}}";

// Create a new HttpClient instance
using (HttpClient client = new())
{
  // Set the authorization header with your access token
  client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
  client.DefaultRequestHeaders.Add("User-Agent", "GitHubContributionScraper");

  // Create a new GraphQL request
  var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
  {
    Content = new StringContent($"{{ \"query\": \"{query.Replace("\n", "\\n").Replace("\"", "\\\"")}\" }}", System.Text.Encoding.UTF8, "application/json")
  };

  // Send the request and get the response
  var response = await client.SendAsync(request);

  // Check if the request was successful
  if (response.IsSuccessStatusCode)
  {
    // Read the response content as a string
    string responseContent = await response.Content.ReadAsStringAsync();

    // Parse the JSON response
    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
    var weeks = jsonResponse.GetProperty("data").GetProperty("viewer").GetProperty("contributionsCollection").GetProperty("contributionCalendar").GetProperty("weeks");

    // Create a StreamWriter to write to the file
    using (StreamWriter writer = new(config.DataFilePath))
    {
      foreach (var week in weeks.EnumerateArray())
      {
        foreach (var day in week.GetProperty("contributionDays").EnumerateArray())
        {
          int contributionCount = day.GetProperty("contributionCount").GetInt32();
          string date = day.GetProperty("date").GetString()!;

          // Write the block data to the file in a single line separated by commas
          writer.WriteLine($"{contributionCount},{DateTime.Parse(date):M/d/yyyy},{(int)DateTime.Parse(date).DayOfWeek}");
        }
      }
    }

    Console.WriteLine($"Data has been written to {config.DataFilePath}");
  }
  else
  {
    // Handle the error response
    Console.WriteLine($"Request failed with status code {response.StatusCode}");
  }

  //Debugging
  // var path = Directory.GetCurrentDirectory();
  // var root = Path.GetDirectoryName(Path.GetDirectoryName(path));
  // var ancestor = Path.GetDirectoryName(Path.GetDirectoryName(root));
  // ancestor += "data.txt";
  // Console.WriteLine(ancestor);
}
