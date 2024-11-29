using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IAssetRepository : IRepositoryBase<Asset>
    {
    }
    public class AssetRepository : RepositoryBase<Asset>, IAssetRepository
    {
        public AssetRepository(DataAccess.ContextAccesss.InSyncContext context) : base(context)
        {
        }
    }
}
