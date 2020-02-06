using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using TorathonGitScraper.Models;

namespace TorathonGitScraper
{
    public class DynamoClient
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public DynamoClient(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task Save(RepoDependencies repoDependencies)
        {
            try
            {
                await _dynamoDb.PutItemAsync(new PutItemRequest("torathon-dependencies",
                    new Dictionary<string, AttributeValue>
                    {
                        {"Name", new AttributeValue(repoDependencies.Name)},
                        {"Dependencies", new AttributeValue(JsonConvert.SerializeObject(repoDependencies.ProjectsDependencies))},
                    }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
