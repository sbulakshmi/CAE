using CAEPOC.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
        //public async Task<string> GetLOINCCode4CPTCode(string cptCode)
        public string GetLOINCCode4CPTCode(string cptCode)
        {
            try
            {
               // int code = int.Parse(cptCode);
                // string code = cptCode;
                return _context.CPT2Loincs
                                .Find(cpt2Loinc => cpt2Loinc.TOEXPR == cptCode)
                                .FirstOrDefault()?.FROMEXPR;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

    }
}
