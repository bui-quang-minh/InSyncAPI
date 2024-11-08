using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface ISubscriptionPlanRepository : IRepositoryBase<SubscriptionPlan>
    {
        Task DeleteSubsciptionPlan(Guid id);
    }
    public class SubscriptionPlanRepository : RepositoryBase<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }

        public async Task DeleteSubsciptionPlan(Guid id)
        {
           var subsciptionPlan = await _context.SubscriptionPlans.Include(u => u.UserSubscriptions)
                .FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (subsciptionPlan == null) return;
            if (subsciptionPlan.UserSubscriptions.Any())
            {
                _context.UserSubscriptions.RemoveRange(subsciptionPlan.UserSubscriptions);
                await _context.SaveChangesAsync();
            }
            _context.SubscriptionPlans.Remove(subsciptionPlan); 
            await _context.SaveChangesAsync();
        }
    }
}
