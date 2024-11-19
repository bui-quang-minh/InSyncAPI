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
    public interface IUserRepository : IRepositoryBase<User>
    {
       
    }
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }

    }
}
