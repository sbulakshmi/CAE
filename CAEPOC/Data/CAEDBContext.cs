using CAEPOC.Interfaces;
using CAEPOC.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Data
{
    public class CAEDBContext : ICAEDBContext
    {
        private readonly IMongoDatabase _database = null;
        public CAEDBContext(IOptions<Settings> settings, IMongoClient client)
        {
            //var client = new MongoClient(settings.Value.ConnectionString);
            //if (client != null)
            _database = client.GetDatabase(settings.Value.Database);
        }
        public IMongoCollection<Edi.Templates.Hipaa5010.TS837P> T837PClaims
        {
            get
            {
                return _database.GetCollection<Edi.Templates.Hipaa5010.TS837P>("T837PClaim");
            }
        }
        public IMongoCollection<CPT2Loinc> CPT2Loincs
        {
            get
            {
                return _database.GetCollection<CPT2Loinc>("CPT2Loinc");
            }
        }
        public IMongoCollection<EdiFabric.Templates.Hipaa5010.TS277> T277
        {
            get
            {
                return _database.GetCollection<EdiFabric.Templates.Hipaa5010.TS277>("T277");
            }
        }

        public IMongoCollection<Counter> Counters
        {
            get
            {
                return _database.GetCollection<Counter>("Counter");
            }
        }

        public IMongoCollection<CustomCPTLoincMapping> CustomCPTLoincMappings
        {
            get
            {
                return _database.GetCollection<CustomCPTLoincMapping>("CustomCPTLoincMappings");
            }
        }
    }
}
