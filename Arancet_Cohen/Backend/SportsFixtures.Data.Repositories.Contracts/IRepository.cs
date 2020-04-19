using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObligatorioDA2.Data.Repositories.Contracts
{
    public interface IRepository<T, K>
    {
        bool IsEmpty();

        T Add(T entity);

        void Delete(K id);

        bool Exists(K record);

        void Clear();

        void Modify(T entity);

        T Get(K id);

        ICollection<T> GetAll();
    }
}
