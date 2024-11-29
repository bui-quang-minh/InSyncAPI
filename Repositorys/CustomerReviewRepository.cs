using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ICustomerReviewRepository : IRepositoryBase<CustomerReview>
    {
    }
    public class CustomerReviewRepository : RepositoryBase<CustomerReview>, ICustomerReviewRepository
    {
        public CustomerReviewRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
