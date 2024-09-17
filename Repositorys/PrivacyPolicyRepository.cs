using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IPrivacyPolicyRepository : IRepositoryBase<PrivacyPolicy>
    {
    }
    public class PrivacyPolicyRepository : RepositoryBase<PrivacyPolicy>, IPrivacyPolicyRepository
    {
        public PrivacyPolicyRepository(InSyncContext context) : base(context)
        {
        }
    }
}
