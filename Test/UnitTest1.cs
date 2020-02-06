using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Test
{
    public class Tests
    {

        [Test]
        public void Test1()
        {
            var xElem = "<PackageReference Include=\"FluentAssertions\" Version=\"5.7.0\" />";

            var regex = new Regex(@"\""(.*?)\""");

            var matches = regex.Matches(xElem);
            Assert.Pass();
        }
    }
}