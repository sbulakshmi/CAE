using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CAEPOC.Interfaces
{
    public interface ICAERepository
    {
        Task AddT837PClaim(Edi.Templates.Hipaa5010.TS837P item);
        //Task <String> GetLOINCCode4CPTCode(string cptCode);
        Task AddT277(EdiFabric.Templates.Hipaa5010.TS277 item);
        String GetLOINCCode4CPTCode(string cptCode);
        Task<List<string>> GetRequestCodes(string cptCode);
        long GetNextSequence(string code);
    }
}
