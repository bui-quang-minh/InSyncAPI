using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ICategoryDocumentRepository : IRepositoryBase<CategoryDocument>
    {
        Task DeleteCategoryDocument(Guid id);
        Task<IEnumerable<CategoryDocument>> GetMultiUpdate(string keySearch);
        IEnumerable<CategoryDocument> GetMultiPagingUpdate(string keySearch, out int total, int index = 0, int size = 20);
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

        public async Task<IEnumerable<CategoryDocument>> GetMultiUpdate(string keySearch)
        {
            var listCategory = await _context.CategoryDocuments
                .Where(c => c.Title.ToLower().Contains(keySearch))
                .Select(c => new CategoryDocument
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Order = c.Order,
                    DateCreated = c.DateCreated,
                    DateUpdated = c.DateUpdated,
                    Documents = c.Documents.Select(d => new Document
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Slug = d.Slug,
                        Order = d.Order
                    }).ToList()
                }).ToListAsync();
            return listCategory;
        }
        //.Where(c => c.Title.ToLower().Contains(keySearch)) .Where(c => EF.Functions.Like(c.Title, $"%{keySearch}%"))
        public  IEnumerable<CategoryDocument> GetMultiPagingUpdate(string keySearch, out int total, int index = 0, int size = 20)
        {
            index = index < 0 ? 0 : index;
            size = size < 0 ? 20 : size;
            int skipCount = index * size;
            var _resetSet =  _context.CategoryDocuments
                             .Where(c => c.Title.ToLower().Contains(keySearch))
                             .Skip(skipCount)
                             .Take(size)
                             .Select(c => new CategoryDocument
                             {
                                 Id = c.Id,
                                 Title = c.Title,
                                 Description = c.Description,
                                 Order = c.Order,
                                 DateCreated = c.DateCreated,
                                 DateUpdated = c.DateUpdated,
                                 Documents = c.Documents.Select(d => new Document
                                 {
                                     Id = d.Id,
                                     Title = d.Title,
                                     Slug = d.Slug,
                                     Order = d.Order
                                 }).ToList()
                             }).ToList();
            total = _resetSet.Count();
            return _resetSet;
        }

    }
}
