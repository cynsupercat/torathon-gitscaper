using System;
using Newtonsoft.Json;

namespace TorathonGitScraper.Models
{
    public class GitHubFile
    {
        public string Name { get; set; }

        [JsonProperty("download_url")]
        public string Url { get; set; }

        public string Path { get; set; }

        public string Type { get; set; }

        public bool IsFile()
        {
            return Type.Equals("file", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDir()
        {
            return Type.Equals("dir", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsCsProj()
        {
            return Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
        }
    }
}
