﻿using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IScenarioRepository : IRepositoryBase<Scenario>
    {
    }
    public class ScenarioRepository : RepositoryBase<Scenario>, IScenarioRepository
    {
        public ScenarioRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
