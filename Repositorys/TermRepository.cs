using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface ITermRepository : IRepositoryBase<Term>
    {
    }
    public class TermRepository : RepositoryBase<Term>, ITermRepository
    {
        public TermRepository(InSyncContext context) : base(context)
        {
        }
    }
}
