﻿using CAEPOC.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Data
{
    public class CAEDBContext: ICAEDBContext
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
    }
}
