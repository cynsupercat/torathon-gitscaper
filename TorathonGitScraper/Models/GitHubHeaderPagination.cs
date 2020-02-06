using System;
using System.Linq;

namespace TorathonGitScraper.Models
{
    public class GitHubHeaderPagination
    {
        public string Url { get; }
        public string Rel { get; }
        public int PageNumber { get; set; }

        public GitHubHeaderPagination(string headerValue)
        {
            var splitVals = headerValue.Split("; ");

            var url = splitVals.First();
            var rel = splitVals[1];

            Url = url.TrimStart('<');
            Url = Url.TrimEnd('>');

            Rel = rel.Substring(rel.IndexOf('"') + 1, (rel.Length - 1) - (rel.IndexOf('"') + 1));

            PageNumber = GetPageNumberFromUrl();
        }

        private int GetPageNumberFromUrl()
        {
            var pageParam = "?page=";
            var indexOfParam = Url.LastIndexOf(pageParam, StringComparison.OrdinalIgnoreCase);

            var pageNumber = Url.Substring(indexOfParam + pageParam.Length, Url.Length - (indexOfParam + pageParam.Length));

            return int.Parse(pageNumber);
        }

        public bool IsLast()
        {
            return Rel.Equals("last", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsNext()
        {
            return Rel.Equals("next", StringComparison.OrdinalIgnoreCase);
        }
    }
}
