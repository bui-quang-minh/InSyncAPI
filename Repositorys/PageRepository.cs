using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IPageRepository : IRepositoryBase<Page>
    {
    }
    public class PageRepository : RepositoryBase<Page>, IPageRepository
    {
        public PageRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
