using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Models
{
    [BsonIgnoreExtraElements]
    public class CPT2Loinc
    {
        public string TOEXPR { get; set; }
        public string FROMEXPR { get; set; }
        
    }
}
