using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface ITutorialRepository : IRepositoryBase<Tutorial>
    {
    }
    public class TutorialRepository : RepositoryBase<Tutorial>, ITutorialRepository
    {
        public TutorialRepository(InSyncContext context) : base(context)
        {
        }
    }
}
