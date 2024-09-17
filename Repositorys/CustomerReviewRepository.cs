using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface ICustomerReviewRepository : IRepositoryBase<CustomerReview>
    {
    }
    public class CustomerReviewRepository : RepositoryBase<CustomerReview>, ICustomerReviewRepository
    {
        public CustomerReviewRepository(InSyncContext context) : base(context)
        {
        }
    }
}
