using System;
using Newtonsoft.Json;

namespace TorathonGitScraper.Models
{
    public class GitHubBlob
    {
        public int Size { get; set; }

        [JsonProperty("content")]
        public string EncodedContent { get; set; }

        public string DecodeBase64String()
        {
            var data = Convert.FromBase64String(EncodedContent);
            var decoded = System.Text.Encoding.ASCII.GetString(data);

            return decoded;
        }
    }
}
