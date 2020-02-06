using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;

namespace TorathonGitScraper.Models
{
    public class PackageReference
    {
        public string PackageName { get; }
        public string Version { get; }

        public PackageReference(XElement elem)
        {
            var elemContent = elem.ToString();

            var regex = new Regex(@"\""(.*?)\""");

            var matches = regex.Matches(elemContent);

            PackageName = matches[0].Value;
            Version = matches[1].Value;
        }
    }

    public class PackageReferencesConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            var refs = value as List<PackageReference>;
            if (refs == null) throw new ArgumentException("Not a package ref object");

            var data = JsonConvert.SerializeObject(refs);

            DynamoDBEntry entry = new Primitive
            {
                Value = data
            };

            return entry;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            var primitive = entry as Primitive;
            if (!(primitive?.Value is string) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            var primitiveString = (string)primitive.Value;
            return JsonConvert.DeserializeObject<List<PackageReference>>(primitiveString);
        }
    }

}
