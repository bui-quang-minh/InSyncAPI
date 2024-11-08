using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IUserSubscriptionRepository : IRepositoryBase<UserSubscription>
    {
    }
    public class UserSubscriptionRepository : RepositoryBase<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
