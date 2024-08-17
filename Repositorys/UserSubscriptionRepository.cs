using BusinessObjects.Models;
using DataAccess;
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
        public UserSubscriptionRepository(InSyncContext context) : base(context)
        {
        }
    }
}
