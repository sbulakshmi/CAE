using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAEPOC.Interfaces
{
    public interface ICAERepository
    {
        Task AddT837PClaim(Edi.Templates.Hipaa5010.TS837P item);
    }
}
