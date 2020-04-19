using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using RepositoryInterface;

namespace RepositoryInterface
{
    public interface IEntityRepository<T>: IRepository<T>
    {
        bool Any(Expression<Func<T, bool>> predicate);

        T First(Expression<Func<T, bool>> predicate);

        ICollection<T> Get(Expression<Func<T, bool>> predicate);
    }
}
