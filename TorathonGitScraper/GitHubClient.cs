using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TorathonGitScraper.Models;

namespace TorathonGitScraper
{
    public class GitHubClient
    {
        private readonly HttpClient _httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GitHubFile>> GetCsProjFiles(GitHubRepo repository)
        {
            var files = new List<GitHubFile>();
            var rootFiles = await GetFiles(repository, "src", f => f.IsDir());

            if (!rootFiles.Any())
                return files;

            // This assumes the solution follows the usual structure
            var secondLevelDirectories = rootFiles.Where(x => x.IsDir());

            var getFilesTasks = new List<Task<List<GitHubFile>>>();

            foreach (var dir in secondLevelDirectories)
            {
                try
                {
                    var getCsProjFilesTask = GetFiles(repository, dir.Path, f => f.IsCsProj());
                    getFilesTasks.Add(getCsProjFilesTask);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error parsing files for {dir}");
                    Console.WriteLine(e);
                }
            }

            var tasksResult = await Task.WhenAll(getFilesTasks);
            var csProjFiles = tasksResult.SelectMany(x => x).ToList();
            return csProjFiles;
        }

        public async Task<GitHubBlob> GetBlob(string path)
        {

            var response = await _httpClient.GetAsync(path);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GitHubBlob>(content);
        }

        public async Task<List<GitHubFile>> GetFiles(GitHubRepo repository, string path, Func<GitHubFile, bool> exp)
        {
            const string pathParam = "{+path}";

            string contentUrl;

            if (string.IsNullOrEmpty(path))
            {
                contentUrl = repository.ContentsUrl.Substring(0,
                    repository.ContentsUrl.Length -
                    pathParam.Length);
            }
            else
            {
                contentUrl = repository.ContentsUrl.Replace(pathParam, path);
            }

            var response = await _httpClient.GetAsync(contentUrl);

            var content = await response.Content.ReadAsStringAsync();

            var files = JsonConvert.DeserializeObject<List<GitHubFile>>(content);
            return files.Where(exp).ToList();
        }

        public async Task<List<GitHubRepo>> GetAllRepos()
        {
            var currentPage = 1;

            var (repos, pagination) = await GetDataWithPagination<List<GitHubRepo>>("orgs/ticklesource/repos");

            if (!pagination.Any())
                return repos;

            var lastPage = pagination.Single(x => x.IsLast()).PageNumber;

            while (currentPage < lastPage)
            {
                var next = pagination.Single(x => x.IsNext());

                var (data, paginations) = await GetDataWithPagination<List<GitHubRepo>>(next.Url);

                repos.AddRange(data);
                pagination = paginations;

                currentPage++;
            }

            return repos;
        }

        private async Task<(T data, List<GitHubHeaderPagination> pagination)> GetDataWithPagination<T>(string uri)
        {
            var result = await _httpClient.GetAsync(uri);
            var contentString = await result.Content.ReadAsStringAsync();

            var paginationHeaders = GetPaginationFromHeader(result);

            var data = JsonConvert.DeserializeObject<T>(contentString);

            return (data, paginationHeaders);
        }

        private static List<GitHubHeaderPagination> GetPaginationFromHeader(HttpResponseMessage response)
        {
            var paginations = new List<GitHubHeaderPagination>();

            if (!response.Headers.TryGetValues("Link", out var vals))
                return paginations;

            var links = vals.First().Split(", ");

            foreach (var link in links)
            {
                paginations.Add(new GitHubHeaderPagination(link));
            }

            return paginations;
        }
    }
}
