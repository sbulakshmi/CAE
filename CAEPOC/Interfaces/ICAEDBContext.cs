using CAEPOC.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Interfaces
{
    public interface ICAEDBContext
    {
        IMongoCollection<Edi.Templates.Hipaa5010.TS837P> T837PClaims { get; }
        IMongoCollection<CPT2Loinc> CPT2Loincs { get; }

        IMongoCollection<EdiFabric.Templates.Hipaa5010.TS277> T277 { get; }
        IMongoCollection<Counter> Counters { get; }
        IMongoCollection<CustomCPTLoincMapping> CustomCPTLoincMappings { get; }
        
    }
}
