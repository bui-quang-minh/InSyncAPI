using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IScenarioRepository : IRepositoryBase<Scenario>
    {
    }
    public class ScenarioRepository : RepositoryBase<Scenario>, IScenarioRepository
    {
        public ScenarioRepository(InSyncContext context) : base(context)
        {
        }
    }
}
