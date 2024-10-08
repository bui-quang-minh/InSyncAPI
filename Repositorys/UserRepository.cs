﻿using BusinessObjects.Models;
using DataAccess.ContextAccesss;
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
        public UserRepository(InSyncContext context) : base(context)
        {
        }
    }
}
