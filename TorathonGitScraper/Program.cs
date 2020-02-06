using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TorathonGitScraper.Models;

namespace TorathonGitScraper
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddHttpClient<GitHubClient>(o =>
            {
                o.BaseAddress = new Uri("https://api.github.com");
                o.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("token", "0d5abdb9a0229594e645244bd888a0dbb2610f27");
                o.DefaultRequestHeaders.Add("User-Agent", "TorathonGitScraper");
            });

            var serviceProvider = services.BuildServiceProvider();

            var gitHubClient = serviceProvider.GetService<GitHubClient>();

            const string allReposFile = @"D:\dev\TorathonGitScraper\repos.json";

            var reposFileInfo = new FileInfo(allReposFile);

            List<GitHubRepo> repos;

            if (!reposFileInfo.Exists)
            {
                Console.WriteLine("Repo file doesn't exist: fetching.");

                repos = await gitHubClient.GetAllRepos();
                using (var writer = new StreamWriter(allReposFile, append: false))
                {
                    await writer.WriteLineAsync(JsonConvert.SerializeObject(repos));
                }

                Console.WriteLine("Finished writing to file.");
            }
            else
            {
                using (var sr = reposFileInfo.OpenText())
                {
                    var content = sr.ReadLine();
                    repos = JsonConvert.DeserializeObject<List<GitHubRepo>>(content);
                }

            }



            var test = await gitHubClient.GetFiles(repos.First(x => x.Name.Contains("WorkspaceService")));
        }
    }
}
