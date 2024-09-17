using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IAssetRepository : IRepositoryBase<Asset>
    {
    }
    public class AssetRepository : RepositoryBase<Asset>, IAssetRepository
    {
        public AssetRepository(InSyncContext context) : base(context)
        {
        }
    }
}
