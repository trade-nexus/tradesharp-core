using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Stereotype;
using Spring.Transaction.Interceptor;
using TradeHub.Common.Core.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Repositories
{
    /// <summary>
    /// Persistacne repository
    /// </summary>
    [Repository]
    public class PersistRepository : HibernateDao,IPersistRepository<object>
    {
        [Transaction(ReadOnly = false)]
        public void AddUpdate(object entity)
        {
            CurrentSession.SaveOrUpdate(entity);
        }

        public void AddUpdate(IEnumerable<object> collection)
        {
            throw new NotImplementedException();
        }

        public void Delete(object entity)
        {
            throw new NotImplementedException();
        }
    }
}
