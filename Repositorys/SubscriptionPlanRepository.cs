using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface ISubscriptionPlanRepository : IRepositoryBase<SubscriptionPlan>
    {
    }
    public class SubscriptionPlanRepository : RepositoryBase<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(InSyncContext context) : base(context)
        {
        }
    }
}
