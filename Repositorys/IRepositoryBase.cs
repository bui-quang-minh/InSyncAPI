using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositorys
{
    public interface IRepositoryBase<T> where T: class
    {
        Task<T> Add(T entity);

        Task Update(T entity);

        Task<T> Delete(T entity);

        Task DeleteMulti(Expression<Func<T, bool>> where);


        Task<T> GetSingleByCondition(Expression<Func<T, bool>> expression, string[] includes = null);

        IEnumerable<T> GetAll(string[] includes = null);

        IEnumerable<T> GetMulti(Expression<Func<T, bool>> predicate, string[] includes = null);

        IEnumerable<T> GetMultiPaging(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50, string[] includes = null);

        Task<int> CountAsync(Expression<Func<T, bool>> where);

        Task<bool> CheckContainsAsync(Expression<Func<T, bool>> predicate);
    }
}
