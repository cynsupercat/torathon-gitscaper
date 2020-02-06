using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TorathonGitScraper.Models;

namespace TorathonGitScraper
{
    public class Program
    {
        private static GitHubClient gitHubClient;

        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("AWS_PROFILE", "ticklelabs");

            var configuration = SetupConfiguration(args);

            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            services.AddHttpClient<GitHubClient>(o =>
            {
                o.BaseAddress = new Uri("https://api.github.com");
                o.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("token", "");
                o.DefaultRequestHeaders.Add("User-Agent", "TorathonGitScraper");
            });

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            services
                .AddTransient<DynamoClient>();

            var serviceProvider = services.BuildServiceProvider();

            gitHubClient = serviceProvider.GetService<GitHubClient>();

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

            var totalRepos = repos.Count;
            const int batchSize = 50;

            var batchesNeeded = (totalRepos / 50) + 1;

            var repoFilesTasks = new Dictionary<string, Task<List<GitHubFile>>>();

            for (var batch = 0; batch <= batchesNeeded; batch++)
            {
                var lowerBound = batch * batchSize;
                var range = lowerBound + batchSize > totalRepos ? totalRepos - lowerBound : batchSize;
                var batchItems = repos.GetRange(lowerBound, range);

                foreach (var item in batchItems)
                {
                    repoFilesTasks.Add(item.Name, gitHubClient.GetCsProjFiles(item));
                }

                batch++;
            }

            var dynamoDbClient = serviceProvider.GetService<DynamoClient>();

            foreach (var (repo, filesTask) in repoFilesTasks)
            {
                try
                {
                    Console.WriteLine(repo);
                    var files = await Task.WhenAll(filesTask);

                    var dependencies = new List<ProjectDependencies>();

                    foreach (var file in files.SelectMany(x => x))
                    {
                        var fileContent = await GetRawFileContent(file);
                        var refs = file.GetAllPackageRefs(fileContent);

                        dependencies.AddRange(refs.Select(x => new ProjectDependencies
                        {
                            Project = file.Name.Replace(".csproj", ""),
                            Dependencies = refs
                        }));
                    }

                    await dynamoDbClient.Save(new RepoDependencies
                    {
                        Name = repo,
                        ProjectsDependencies = dependencies
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to get file content for repo {repo}");
                }
            }

        }

        private static async Task<string> GetRawFileContent(GitHubFile file)
        {
            var blob = await gitHubClient.GetBlob(file.Url);

            return blob.DecodeBase64String().Replace("???", "");
        }

        private static IConfiguration SetupConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }
    }
}
