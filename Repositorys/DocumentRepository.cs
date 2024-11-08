using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IDocumentRepository : IRepositoryBase<Document>
    {
    }
    public class DocumentRepository : RepositoryBase<Document>, IDocumentRepository
    {
        public DocumentRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
