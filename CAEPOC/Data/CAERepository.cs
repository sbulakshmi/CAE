using CAEPOC.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Data
{
    public class CAERepository: ICAERepository
    {
        private readonly ICAEDBContext _context;// = null;
        public CAERepository(IOptions<Settings> settings, ICAEDBContext context)
        {
            // _context = new CAEDBContext(settings);
            _context = context;
        }


        public async Task AddT837PClaim(Edi.Templates.Hipaa5010.TS837P item)
        {
            try
            {
                await _context.T837PClaims.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

    }
}
