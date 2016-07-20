using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories
{
    public class IOrderSystemRepository<T>
    {
        void Add(T item);
        void Delete(T item);
        void Update(T item);
        IList<T> RetieveAll<T>();
        T FindByID(object id);
    }
}
