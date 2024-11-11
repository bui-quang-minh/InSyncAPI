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
    public interface ICategoryDocumentRepository : IRepositoryBase<CategoryDocument>
    {
        Task DeleteCategoryDocument(Guid id);
    }
    public class CategoryDocumentRepository : RepositoryBase<CategoryDocument>, ICategoryDocumentRepository
    {
        public CategoryDocumentRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }

        public async Task DeleteCategoryDocument(Guid id)
        {
            
            var deleteCate = await _context.CategoryDocuments.Include(p => p.Documents)
              .FirstOrDefaultAsync(c => c.Id.Equals(id));
            if (deleteCate == null) return;
            if (deleteCate.Documents.Any())
            {
                _context.Documents.RemoveRange(deleteCate.Documents);
                await _context.SaveChangesAsync();
            }
            _context.CategoryDocuments.Remove(deleteCate);
            await _context.SaveChangesAsync();
        }
    }
}
