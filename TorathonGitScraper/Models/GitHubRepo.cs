using Newtonsoft.Json;

namespace TorathonGitScraper.Models
{
    public class GitHubRepo
    {
        public string Id { get; set; }

        [JsonProperty("full_name")]
        public string Path { get; set; }

        public string Name { get; set; }

        [JsonProperty("contents_url")]
        public string ContentsUrl { get; set; }
    }
}
