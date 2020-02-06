using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TorathonGitScraper.Models
{
    public class GitHubFile
    {
        public string Name { get; set; }

        [JsonProperty("git_url")]
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

        public List<PackageReference> GetAllPackageRefs(string content)
        {
            var refs = new List<PackageReference>();

            if (!IsFile())
                return refs;

            using (var stream = FromString(content))
            {
                var projDefinition = XDocument.Load(stream);

                try
                {
                    var packageReferences = projDefinition.Descendants("PackageReference");

                    refs.AddRange(packageReferences.Select(x => new PackageReference(x)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return refs;
        }

        //private string DecodeContent()
        //{
        //    var data = Convert.FromBase64String(Base64EncodedContent);
        //    var decoded = System.Text.Encoding.ASCII.GetString(data);

        //    return decoded;
        //}

        private static Stream FromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(s);
            writer.Flush();

            stream.Position = 0;

            return stream;
        }
    }
}
