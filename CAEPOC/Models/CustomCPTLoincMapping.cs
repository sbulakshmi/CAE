using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Models
{
    [BsonIgnoreExtraElements]
    public class CustomCPTLoincMapping
    {
        public string cptCode { get; set; }
        public string LOINC { get; set; }
    }
}
