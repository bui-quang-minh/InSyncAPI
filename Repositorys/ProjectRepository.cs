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
    public interface IProjectRepository : IRepositoryBase<Project>
    {
        Task DeleteProject(Guid id);
    }
    public class ProjectRepository : RepositoryBase<Project>, IProjectRepository
    {
        public ProjectRepository(InSyncContext context) : base(context)
        {
        }

        public async Task DeleteProject(Guid id)
        {
            var deleteProject = await _context.Projects.Include(p => p.Scenarios)
                .Include(p => p.Assets).FirstOrDefaultAsync(c => c.Id.Equals(id));
            if (deleteProject == null) return;
            if (deleteProject.Scenarios.Any())
            {
                _context.Scenarios.RemoveRange(deleteProject.Scenarios);
                _context.SaveChangesAsync();
            }
            if (deleteProject.Assets.Any())
            {
                _context.Assets.RemoveRange(deleteProject.Assets);
                await _context.SaveChangesAsync();
            }
            _context.Projects.Remove(deleteProject);
            await _context.SaveChangesAsync();

        }
    }
}
