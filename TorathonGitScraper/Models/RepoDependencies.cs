using System.Collections.Generic;

namespace TorathonGitScraper.Models
{
    public class RepoDependencies
    {
        public string Name { get; set; }
        public List<ProjectDependencies> ProjectsDependencies { get; set; }
    }

    public class ProjectDependencies
    {
        public string Project { get; set; }
        public List<PackageReference> Dependencies { get; set; }
    }
}
