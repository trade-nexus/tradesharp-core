using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories
{
    /// <summary>
    /// Reposistory Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        void Add(T item);
        void Delete(T item);
        void Update(T item);
        IList<T> FindAll();
        T FindByID(object id);
    }
}
