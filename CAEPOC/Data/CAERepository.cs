using CAEPOC.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
        public async Task AddT277(EdiFabric.Templates.Hipaa5010.TS277 item)
        {
            try
            {
                await _context.T277.InsertOneAsync(item);
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

        public async Task<List<string>> GetRequestCodes(string cptCode)
        {
            try
            {
                // int code = int.Parse(cptCode);
                // string code = cptCode;
                List<string> reqCode = new List<string>();
                Action<string> add = x => { if (!String.IsNullOrEmpty(x)) reqCode.Add(x); };

                add(_context.CPT2Loincs
                                .Find(cpt2Loinc => cpt2Loinc.TOEXPR == cptCode)
                                .FirstOrDefault()?.FROMEXPR);

                await _context.CustomCPTLoincMappings
                        .Find(cpt2Loinc => cpt2Loinc.cptCode == cptCode || cpt2Loinc.cptCode == "*")
                        .ForEachAsync(x => add(x.LOINC));

                //add("18776-5");
                //add("18776-5");
                return reqCode;
                // return new List<string> { "18776-5 ", "80745-3", "90823" };
                //return _context.CPT2Loincs
                //                .Find(cpt2Loinc => cpt2Loinc.TOEXPR == cptCode)
                //                .FirstOrDefault()?.FROMEXPR;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

       

        public long GetNextSequence(string code)
        {
            try
            {

                // int code = int.Parse(cptCode);
                // string code = cptCode;
                var counterQuery = Builders<Models.Counter>.Filter.Eq("Id", code);
                var option = new FindOneAndUpdateOptions<Models.Counter, Models.Counter> { IsUpsert=true, ReturnDocument = ReturnDocument.After };
                var result = _context.Counters.FindOneAndUpdate(counterQuery, Builders<Models.Counter>.Update.Inc("Value", 1), option);
                return result.Value;

            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }


    }
}
